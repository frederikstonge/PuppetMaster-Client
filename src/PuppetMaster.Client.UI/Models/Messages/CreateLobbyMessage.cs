using System;
using PuppetMaster.Client.Valorant.Api.Models.Requests;

namespace PuppetMaster.Client.UI.Models.Messages
{
    public class CreateLobbyMessage
    {
        public Guid MatchId { get; set; }
    }
}
