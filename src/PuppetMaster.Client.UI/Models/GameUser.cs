using System;

namespace PuppetMaster.Client.UI.Models
{
    public class GameUser
    {
        public Guid Id { get; set; }

        public string UniqueGameId { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;

        public Guid ApplicationUserId { get; set; }

        public User? ApplicationUser { get; set; }

        public Guid GameId { get; set; }

        public Game? Game { get; set; }
    }
}
