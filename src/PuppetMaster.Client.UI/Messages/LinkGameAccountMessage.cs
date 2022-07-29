using System;
using System.Threading.Tasks;

namespace PuppetMaster.Client.UI.Messages
{
    public class LinkGameAccountMessage
    {
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public Func<Task>? OnSuccess { get; set; }
    }
}
