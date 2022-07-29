using System;

namespace PuppetMaster.Client.UI.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public Guid? RoomUserId { get; set; }

        public Guid? RoomId { get; set; }
    }
}
