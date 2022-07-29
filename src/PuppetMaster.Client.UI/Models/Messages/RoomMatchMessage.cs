using System;
using System.Collections.Generic;

namespace PuppetMaster.Client.UI.Models.Messages
{
    public class RoomMatchMessage
    {
        public Match? Match { get; set; }

        public Guid? CaptainToPickThisTurn { get; set; }

        public List<User> AvailablePlayers { get; set; } = new List<User>();

        public TimeSpan? Delay { get; set; }
    }
}
