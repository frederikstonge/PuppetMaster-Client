using System;

namespace PuppetMaster.Client.UI.Models.Events
{
    public class JoinLobbyEventArgs : EventArgs
    {
        public JoinLobbyEventArgs(string lobbyId, Guid matchId, int teamIndex)
        {
            LobbyId = lobbyId;
            MatchId = matchId;
            TeamIndex = teamIndex;
        }

        public string LobbyId { get; set; }

        public Guid MatchId { get; set; }

        public int TeamIndex { get; set; }
    }
}
