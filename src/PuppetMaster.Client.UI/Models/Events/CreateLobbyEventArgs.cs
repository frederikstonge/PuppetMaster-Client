using System;

namespace PuppetMaster.Client.UI.Models.Events
{
    public class CreateLobbyEventArgs : EventArgs
    {
        public CreateLobbyEventArgs(Guid matchId)
        {
            MatchId = matchId;
        }

        public Guid MatchId { get; set; }
    }
}
