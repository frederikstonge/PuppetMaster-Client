using System;

namespace PuppetMaster.Client.UI.Models
{
    public class MatchTeamUser
    {
        public Guid Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public Guid? ModifiedBy { get; set; }

        public Guid ApplicationUserId { get; set; }

        public User? ApplicationUser { get; set; }

        public Guid MatchTeamId { get; set; }

        public bool IsCaptain { get; set; }

        public string? MapVote { get; set; }

        public bool HasJoined { get; set; }
    }
}
