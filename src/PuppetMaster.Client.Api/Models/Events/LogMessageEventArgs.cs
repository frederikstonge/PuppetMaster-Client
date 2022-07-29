namespace PuppetMaster.Client.Valorant.Api.Models.Events
{
    public class LogMessageEventArgs : EventArgs
    {
        public string Message { get; set; } = string.Empty;
    }
}
