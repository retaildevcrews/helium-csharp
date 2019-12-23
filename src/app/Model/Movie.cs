using Newtonsoft.Json;
using System.Collections.Generic;

namespace Helium.Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "breaks json serialization")]
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
        public List<string> Genres { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "breaks json serialization")]
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
        public List<Role> Roles { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "breaks json serialization")]
    public class Role : ActorBase
    {
        [JsonProperty(Order = 1)]
        public int Order { get; set; }
        [JsonProperty(Order = 98)]
        public string Category { get; set; }
        [JsonProperty(Order = 99, NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Characters { get; set; }
    }

    public class FeaturedMovie
    {
        public string MovieId { get; set; }
        public int Weight { get; set; } = 1;
    }
}
