namespace PuppetMaster.Client.UI.Models
{
    public class Token
    {
        public string IdentityToken { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public int ExpiresIn { get; set; }
    }
}
