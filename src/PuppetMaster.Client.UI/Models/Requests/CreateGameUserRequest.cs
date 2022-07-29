using System;

namespace PuppetMaster.Client.UI.Models.Requests
{
    public class CreateGameUserRequest
    {
        public string UniqueGameId { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;

        public Guid GameId { get; set; }
    }
}
