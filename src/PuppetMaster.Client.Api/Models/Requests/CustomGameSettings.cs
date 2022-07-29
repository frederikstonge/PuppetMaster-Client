namespace PuppetMaster.Client.Valorant.Api.Models.Requests
{
    public class CustomGameSettings
    {
        public string Map { get; set; } = string.Empty;

        public string Mode { get; set; } = string.Empty;

        public bool UseBots { get; set; }

        public string GamePod { get; set; } = string.Empty;

        public GameRules GameRules { get; set; } = new GameRules();
    }
}
