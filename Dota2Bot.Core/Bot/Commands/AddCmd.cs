﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dota2Bot.Core.Domain;
using Dota2Bot.Core.Engine;
using Dota2Bot.Core.OpenDota;
using Dota2Bot.Domain.Entity;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Dota2Bot.Core.Bot.Commands
{
    public class AddCmd : BaseCmd
    {
        public override string Cmd => "add";
        public override string Description => "add subscription to dotabuff profile by id or link";

        protected override async Task ExecuteHandler(long chatId, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                await Telegram.SendTextMessageAsync(chatId, "Please specify dotabuff link or id");
                return;
            }

            var playerId = GetPlayerId(args);
            if (playerId == 0)
            {
                await Telegram.SendTextMessageAsync(chatId, "Invalid dotabuff link or id");
                return;
            }
            
            await Telegram.SendTextMessageAsync(chatId, "Saving... ");

            var player = await DataManager.PlayerGet(playerId);
            if (player == null)
            {
                player = await CollectPlayerData(DataManager, OpenDota, playerId);
                if (player == null)
                {
                    await Telegram.SendTextMessageAsync(chatId, "Player not found");
                    return;
                }
            }

            var chat = await DataManager.ChatGetOrAdd(chatId, x => x.ChatPlayers);
            var added = await DataManager.ChatAddPlayer(chat, player);
            
            await DataManager.SaveChangesAsync();

            var msg = added
                ? $"Player *{player.Name}* successfully added"
                : $"Player *{player.Name}* already in chat";

            await Telegram.SendTextMessageAsync(chatId, msg, parseMode: ParseMode.Markdown);
        }

        private async Task<Player> CollectPlayerData(DataManager dataManager, OpenDotaClient openDota, long playerId)
        {
            var info = await openDota.Player(playerId);
            if (info == null) 
                return null;

            var player = dataManager.Players
                .Add(new Player { Id = playerId })
                .Entity;

            // основное инфо
            player.Name = info.profile.personaname;
            player.SteamId = info.profile.steamid;
            player.SoloRank = info.solo_competitive_rank;
            player.PartyRank = info.competitive_rank;
            player.RankTier = info.rank_tier;

            // матчи
            var matches = await GetMatches(openDota, player);
            if (matches.Count > 0)
            {
                dataManager.Matches.AddRange(matches);

                var latest = matches.OrderByDescending(x => x.MatchId).First();

                player.LastMatchId = latest.MatchId;
                player.LastMatchDate = latest.DateStart + latest.Duration;
            }

            player.WinCount = matches.Count(x => x.Won);
            player.LoseCount = matches.Count(x => !x.Won);

            // рейтинги
            var ratings = await GetRatings(openDota, player);
            if (ratings.Count > 0)
            {
                dataManager.Ratings.AddRange(ratings);

                player.LastRatingDate = ratings.Max(x => x.Date);
            }

            return player;
        }

        private async Task<List<Match>> GetMatches(OpenDotaClient openDota, Player player)
        {
            const int CollectLimit = 10;

            var limit = player.LastMatchId == null ? null : (int?) CollectLimit;

            var matches = (await openDota.Matches(player.Id, limit: limit))
                .TakeWhile(x => x.match_id != player.LastMatchId) //todo where?
                .ToList();

            return matches.Select(match => new Match
            {
                MatchId = match.match_id,
                PlayerId = match.account_id,
                PlayerSlot = match.player_slot,
                HeroId = match.hero_id,
                GameMode = match.game_mode,
                LobbyType = match.lobby_type,
                DateStart = match.DateStart(),
                Duration = match.Duration(),
                Kills = match.kills,
                Deaths = match.deaths,
                Assists = match.assists,
                Won = match.Won(),
                LeaverStatus = match.leaver_status
            }).ToList();
        }

        private async Task<List<Rating>> GetRatings(OpenDotaClient openDota, Player player)
        {
            var lastRatingDate = player.LastRatingDate ?? DateTime.MinValue;

            var ratings = (await openDota.Ratings(player.Id))
                .Where(x => x.time > lastRatingDate)
                .ToList();

            return ratings.Select(x => new Rating
            {
                PlayerId = x.account_id,
                MatchId = x.match_id,
                SoloRank = x.solo_competitive_rank,
                PartyRank = x.competitive_rank,
                Date = x.time
            }).ToList();
        }
    }
}
