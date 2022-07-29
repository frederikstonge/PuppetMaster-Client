namespace PuppetMaster.Client.Valorant.Api.Models.Internal
{
    internal static class Constants
    {
        public const string ClientPlatform = "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9";

        public const string GameVersionLineInfo = "LogShooter: Display: CI server version:";

        public static readonly string LockfileDataPath = Path.Combine(
                                                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                            @"Riot Games\Riot Client\Config\lockfile");

        public static readonly string ShooterGameLogPath = Path.Combine(
                                                               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                               @"VALORANT\Saved\Logs\ShooterGame.log");
    }
}
