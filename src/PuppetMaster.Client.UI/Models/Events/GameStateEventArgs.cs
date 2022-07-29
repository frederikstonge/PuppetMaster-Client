using System;

namespace PuppetMaster.Client.UI.Models.Events
{
    public class GameStateEventArgs : EventArgs
    {
        public GameStateEventArgs(bool isRunning)
        {
            IsRunning = isRunning;
        }

        public bool IsRunning { get; set; }
    }
}
