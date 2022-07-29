using System;
using PuppetMaster.Client.Valorant.Api.Models.Requests;

namespace PuppetMaster.Client.UI.Models.Messages
{
    public class SetupLobbyMessage
    {
        public GameMap Map { get; set; }

        public Guid MatchId { get; set; }
    }
}
