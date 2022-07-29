using System;

namespace PuppetMaster.Client.UI.Models.Requests
{
    public class CreateRoomRequest
    {
        public string Name { get; set; } = string.Empty;

        public Guid GameId { get; set; }

        public string? Password { get; set; }
    }
}
