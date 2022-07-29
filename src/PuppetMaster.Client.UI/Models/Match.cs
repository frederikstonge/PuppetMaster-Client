﻿using System;
using System.Collections.Generic;

namespace PuppetMaster.Client.UI.Models
{
    public class Match
    {
        public Guid Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public Guid? ModifiedBy { get; set; }

        public string Region { get; set; } = string.Empty;

        public List<MatchTeam>? MatchTeams { get; set; }

        public Guid GameId { get; set; }

        public Game? Game { get; set; }

        public Guid? RoomId { get; set; }

        public Guid? LobbyLeaderId { get; set; }

        public string? LobbyId { get; set; }
    }
}
