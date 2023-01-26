using System;

namespace PuppetMaster.Client.UI.Models.Messages
{
    public class SetupLobbyMessage
    {
        public string? Map { get; set; }

        public Guid MatchId { get; set; }
    }
}
