namespace PuppetMaster.Client.Valorant.Api.Models.Internal
{
    internal class PartyMember
    {
        public string Subject { get; set; } = string.Empty;

        public List<GamePod> Pings { get; set; } = new List<GamePod>();
    }
}
