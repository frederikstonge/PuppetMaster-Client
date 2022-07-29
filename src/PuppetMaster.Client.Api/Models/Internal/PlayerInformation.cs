using Newtonsoft.Json;

namespace PuppetMaster.Client.Valorant.Api.Models.Internal
{
    public class PlayerInformation
    {
        public bool Federated { get; set; }

        [JsonProperty("game_name")]
        public string GameName { get; set; } = string.Empty;

        [JsonProperty("game_tag")]
        public string GameTag { get; set; } = string.Empty;

        public bool Loaded { get; set; }

        public string Name { get; set; } = string.Empty;

        [JsonProperty("pid")]
        public string PlayerId { get; set; } = string.Empty;

        [JsonProperty("puuid")]
        public string PlayerUniqueId { get; set; } = string.Empty;
    }
}
