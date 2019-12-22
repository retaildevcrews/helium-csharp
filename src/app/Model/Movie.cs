using Newtonsoft.Json;
using System.Collections.Generic;

namespace Helium.Model
{
    public class MovieBase
    {
        [JsonProperty(Order = 2)]
        public string MovieId { get; set; }
        [JsonProperty(Order = 5)]
        public string Type { get; set; }
        [JsonProperty(Order = 6)]
        public string Title { get; set; }
        [JsonProperty(Order = 8)]
        public int Year { get; set; }
        [JsonProperty(Order = 9)]
        public int Runtime { get; set; }
        [JsonProperty(Order = 30, NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Genres { get; }
    }

    public class Movie : MovieBase
    {
        [JsonProperty(Order = 1)]
        public string Id { get; set; }
        [JsonProperty(Order = 3)]
        public string PartitionKey { get; set; }
        [JsonProperty(Order = 21)]
        public double Rating { get; set; }
        [JsonProperty(Order = 22)]
        public long Votes { get; set; }
        [JsonProperty(Order = 23)]
        public long TotalScore { get; set; }
        [JsonProperty(Order = 7)]
        public string TextSearch { get; set; }
        [JsonProperty(Order = 99, NullValueHandling = NullValueHandling.Ignore)]
        public List<Role> Roles { get; }
    }

    public class Role : ActorBase
    {
        [JsonProperty(Order = 1)]
        public int Order { get; set; }
        [JsonProperty(Order = 98)]
        public string Category { get; set; }
        [JsonProperty(Order = 99, NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Characters { get; }
    }

    public class FeaturedMovie
    {
        public string movieId { get; set; }
        public int weight { get; set; } = 1;
    }
}
