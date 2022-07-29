using System;

namespace PuppetMaster.Client.UI.Models
{
    public class RoomUser
    {
        public Guid Id { get; set; }

        public Guid ApplicationUserId { get; set; }

        public User? ApplicationUser { get; set; }

        public Guid RoomId { get; set; }

        public bool IsReady { get; set; }
    }
}
