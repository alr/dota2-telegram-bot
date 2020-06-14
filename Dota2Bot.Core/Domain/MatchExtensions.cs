using System;
using System.Collections.Generic;
using System.Linq;
using Dota2Bot.Domain.Entity;
using Dota2Bot.Domain.Enums;

namespace Dota2Bot.Core.Domain
{
    public static class MatchExtensions
    {
        public static string KdaStr(this Match m)
        {
            return String.Format("{0}/{1}/{2}", m.Kills, m.Deaths, m.Assists);
        }

        public static string GameModeStr(this Match m)
        {
            return GameMode.Get(m.GameMode);
        }

        public static string LobbyTypeStr(this Match m)
        {
            return LobbyType.Get(m.LobbyType);
        }

        public static string WonStr(this Match m)
        {
            if (m.LeaverStatus != 0)
                return LeaverStatus.Get(m.LeaverStatus);

            return m.Won ? "Won Match" : "Lost Match";
        }

        public static GameResult GameResult(this Match m)
        {
            if (m.LeaverStatus >= 2)
                return Dota2Bot.Domain.Enums.GameResult.Abandoned;

            return m.Won ? Dota2Bot.Domain.Enums.GameResult.Won : Dota2Bot.Domain.Enums.GameResult.Lost;
        }

        public static string Link(this Match m)
        {
            return "http://www.dotabuff.com/matches/" + m.MatchId;
        }

        public static double WinRate(this IEnumerable<Match> collection)
        {
            var matchHistories = collection as IList<Match> ?? collection.ToList();
            return matchHistories.Count(x => x.Won) * 1.0 / matchHistories.Count();
        }
    }
}
