using System;
using System.Collections.Generic;

namespace PuppetMaster.Client.UI.Models
{
    public class Game
    {
        public Guid Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public Guid? ModifiedBy { get; set; }

        public string Name { get; set; } = string.Empty;

        public string IconUrl { get; set; } = string.Empty;

        public int TeamCount { get; set; }

        public int PlayerCount { get; set; }

        public List<Map> Maps { get; set; } = new List<Map>();
    }
}
