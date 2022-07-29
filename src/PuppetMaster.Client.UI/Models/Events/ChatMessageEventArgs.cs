using System;
using PuppetMaster.Client.UI.Models.Messages;

namespace PuppetMaster.Client.UI.Models.Events
{
    public class ChatMessageEventArgs : EventArgs
    {
        public ChatMessageEventArgs(ChatMessage chatMessage)
        {
            ChatMessage = chatMessage;
        }

        public ChatMessage ChatMessage { get; set; }
    }
}
