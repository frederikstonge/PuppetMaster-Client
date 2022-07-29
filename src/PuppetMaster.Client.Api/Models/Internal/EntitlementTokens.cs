using Newtonsoft.Json;

namespace PuppetMaster.Client.Valorant.Api.Models.Internal
{
    internal class EntitlementTokens
    {
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("token")]
        public string EntitlementToken { get; set; } = string.Empty;

        [JsonProperty("subject")]
        public string UserPlayerId { get; set; } = string.Empty;
    }
}
