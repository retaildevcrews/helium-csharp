using Newtonsoft.Json;
using System.Collections.Generic;

namespace Helium.Model
{
    public class MovieBase
    {
        [JsonProperty(Order = 2)]
        public string MovieId;
        [JsonProperty(Order = 5)]
        public string Type;
        [JsonProperty(Order = 6)]
        public string Title;
        [JsonProperty(Order = 8)]
        public int Year;
        [JsonProperty(Order = 9)]
        public int Runtime;
        [JsonProperty(Order = 30, NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Genres;
    }

    public class Movie : MovieBase
    {
        [JsonProperty(Order = 1)]
        public string Id;
        [JsonProperty(Order = 3)]
        public string PartitionKey;
        [JsonProperty(Order = 21)]
        public double Rating;
        [JsonProperty(Order = 22)]
        public long Votes;
        [JsonProperty(Order = 23)]
        public long TotalScore;
        [JsonProperty(Order = 7)]
        public string TextSearch;
        [JsonProperty(Order = 99, NullValueHandling = NullValueHandling.Ignore)]
        public List<Role> Roles;
    }

    public class Role : ActorBase
    {
        [JsonProperty(Order = 1)]
        public int Order;
        [JsonProperty(Order = 98)]
        public string Category;
        [JsonProperty(Order = 99, NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Characters;
    }
}
