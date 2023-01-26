using System;

namespace PuppetMaster.Client.UI.Models.Events
{
    public class SetupLobbyEventArgs : EventArgs
    {
        public SetupLobbyEventArgs(string map, Guid matchId)
        {
            Map = map;
            MatchId = matchId;
        }

        public string Map { get; set; }

        public Guid MatchId { get; set; }
    }
}
