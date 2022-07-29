using System;
using PuppetMaster.Client.UI.Models.Messages;

namespace PuppetMaster.Client.UI.Models.Events
{
    public class MatchChangedEventArgs : EventArgs
    {
        public MatchChangedEventArgs(RoomMatchMessage match)
        {
            Match = match;
        }

        public RoomMatchMessage Match { get; set; }
    }
}
