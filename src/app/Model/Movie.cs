using System.Collections.Generic;

namespace Helium.Model
{
    public class Movie
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public string MovieId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public int Runtime { get; set; }
        public List<string> Genres { get; set; }
        public double Rating { get; set; }
        public long Votes { get; set; }
        public long TotalScore { get; set; }
        public string TextSearch { get; set; }
        public List<Role> Roles { get; set; }
    }

    public class Role
    {
        public int Order { get; set; }
        public string ActorId { get; set; }
        public string Name { get; set; }
        public int BirthYear { get; set; }
        public int? DeathYear { get; set; }
        public string Category { get; set; }
        public List<string> Characters { get; set; }
    }

    public class FeaturedMovie
    {
        public string MovieId { get; set; }
        public int Weight { get; set; } = 1;
    }
}
