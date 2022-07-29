using System;

namespace PuppetMaster.Client.UI.Models.Events
{
    public class RoomChangedEventArgs : EventArgs
    {
        public RoomChangedEventArgs(Room room)
        {
            Room = room;
        }

        public Room Room { get; set; }
    }
}
