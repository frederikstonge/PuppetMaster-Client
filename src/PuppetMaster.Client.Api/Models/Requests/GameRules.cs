namespace PuppetMaster.Client.Valorant.Api.Models.Requests
{
    public class GameRules
    {
        public string AllowGameModifiers { get; set; } = string.Empty;

        public string PlayOutAllRounds { get; set; } = string.Empty;

        public string SkipMatchHistory { get; set; } = string.Empty;

        public string TournamentMode { get; set; } = string.Empty;

        public string IsOvertimeWinByTwo { get; set; } = string.Empty;
    }
}
