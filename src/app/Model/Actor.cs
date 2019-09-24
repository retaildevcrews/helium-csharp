using Newtonsoft.Json;
using System.Collections.Generic;

namespace Helium.Model
{
    public class ActorBase
    {
        [JsonProperty(Order = 2)]
        public string ActorId;
        [JsonProperty(Order = 10)]
        public string Name;
        [JsonProperty(Order = 11)]
        public int BirthYear;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Order = 12)]
        public int DeathYear;
    }

    public class Actor : ActorBase
    {
        [JsonProperty(Order = 1)]
        public string Id;
        [JsonProperty(Order = 3)]
        public string PartitionKey;
        [JsonProperty(Order = 96)]
        public List<string> Profession;
        [JsonProperty(Order = 97)]
        public string Type;
        [JsonProperty(Order = 98)]
        public string TextSearch;
        [JsonProperty(Order = 99)]
        public List<MovieBase> Movies;
    }
}
