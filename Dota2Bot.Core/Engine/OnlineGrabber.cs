using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dota2Bot.Core.Engine.Models;

namespace Dota2Bot.Core.Engine
{
    public class OnlineGrabber
    {
        private readonly MatchNotifier matchNotifier;
        private readonly string gameTitle;
        private List<string> prevOnline = new List<string>();
        private Dictionary<string, PlayerGameSession> playersTimeCache = new Dictionary<string, PlayerGameSession>();

        public OnlineGrabber(MatchNotifier matchNotifier,
            string gameTitle)
        {
            this.matchNotifier = matchNotifier;
            this.gameTitle = gameTitle;
        }

        public async Task CheckOnline(List<string> currOnline)
        { 
            if (prevOnline == null)
            {
                prevOnline = currOnline;
                return;
            }

            var now = DateTime.UtcNow;
            
            var newOnline = currOnline.Except(prevOnline).ToList();
            var newOffline = prevOnline.Except(currOnline).ToList();

            prevOnline = currOnline;
            
            foreach (var online in newOnline)
            {
                playersTimeCache[online] = new PlayerGameSession
                {
                    SteamId = online,
                    Start = now
                };
            }
            
            List<PlayerGameSession> endSessions = new List<PlayerGameSession>();
            foreach (var offline in newOffline)
            {
                if (playersTimeCache.ContainsKey(offline))
                {
                    var session = playersTimeCache[offline];
                    session.End = now;

                    endSessions.Add(session);

                    playersTimeCache.Remove(offline);
                }
                else
                {
                    endSessions.Add(new PlayerGameSession
                    {
                        SteamId = offline
                    });
                }
            }

            await matchNotifier.NotifyStatusChats(newOnline, endSessions, gameTitle);
        }
    }
}