using System;
using System.Collections.Generic;

namespace PuppetMaster.Client.UI.Models
{
    public class Room
    {
        public Guid Id { get; set; }

        public string Region { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public Guid? ModifiedBy { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<RoomUser>? RoomUsers { get; set; }

        public Guid GameId { get; set; }

        public Game? Game { get; set; }

        public Guid? MatchId { get; set; }

        public bool IsPrivate { get; set; }

        public string DisplayStatus
        {
            get
            {
                if (MatchId.HasValue)
                {
                    return "In-game";
                }

                return RoomUsers!.Count < Game!.PlayerCount ? "Open" : "Full";
            }
        }

        public string DisplayPlayers => $"{RoomUsers!.Count}/{Game!.PlayerCount}";
    }
}
