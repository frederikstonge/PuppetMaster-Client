using System;
using System.Collections.Generic;

namespace PuppetMaster.Client.UI.Models
{
    public class MatchTeam
    {
        public Guid Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public Guid? ModifiedBy { get; set; }

        public int TeamIndex { get; set; }

        public Guid MatchId { get; set; }

        public List<MatchTeamUser>? MatchTeamUsers { get; set; }
    }
}
