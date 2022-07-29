using Newtonsoft.Json;

namespace PuppetMaster.Client.Valorant.Api.Models.Internal
{
    internal class PlayerAffinity
    {
        public Affinities Affinities { get; set; } = new Affinities();

        [JsonIgnore]
        public string Shard => Affinities.Live;

        [JsonIgnore]
        public string Region
        {
            get
            {
                if (Shard == Shards.Brazil || Shard == Shards.LatinAmerica)
                {
                    return Shards.NorthAmerica;
                }

                return Shard;
            }
        }
    }
}
