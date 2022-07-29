namespace PuppetMaster.Client.Valorant.Api.Models.Internal
{
    internal class LockfileData
    {
        public string ProcessName { get; set; } = string.Empty;

        public int ProcessId { get; set; }

        public int Port { get; set; }

        public string Password { get; set; } = string.Empty;

        public string Protocol { get; set; } = string.Empty;
    }
}
