using Newtonsoft.Json;

namespace PuppetMaster.Client.Valorant.Api.Models.Requests
{
    internal class ChangeTeamRequest
    {
        [JsonProperty("playerToPutOnTeam")]
        public string Subject { get; set; } = string.Empty;
    }
}
