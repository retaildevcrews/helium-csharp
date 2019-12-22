using Newtonsoft.Json;
using System.Collections.Generic;

namespace Helium.Model
{
    public class ActorBase
    {
        [JsonProperty(Order = 2)]
        public string ActorId { get; set; }
        [JsonProperty(Order = 10)]
        public string Name { get; set; }
        [JsonProperty(Order = 11)]
        public int BirthYear { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, Order = 12)]
        public int DeathYear { get; set; }
    }

    public class Actor : ActorBase
    {
        [JsonProperty(Order = 1)]
        public string Id { get; set; }
        [JsonProperty(Order = 3)]
        public string PartitionKey { get; set; }
        [JsonProperty(Order = 96)]
        public List<string> Profession { get; }
        [JsonProperty(Order = 97)]
        public string Type { get; set; }
        [JsonProperty(Order = 98)]
        public string TextSearch { get; set; }
        [JsonProperty(Order = 99, NullValueHandling = NullValueHandling.Ignore)]
        public List<MovieBase> Movies { get; }
    }
}
