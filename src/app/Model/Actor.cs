using System.Collections.Generic;

namespace CSE.Helium.Model
{
    public class Actor
    {
        public string Id { get; set; }
        public string ActorId { get; set; }
        public string PartitionKey { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int? BirthYear { get; set; }
        public int? DeathYear { get; set; }
        public string TextSearch { get; set; }
        public List<string> Profession { get; set; }
        public List<ActorMovie> Movies { get; set; }
    }

    public class ActorMovie
    {
        public string MovieId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public int Runtime { get; set; }
        public List<string> Genres { get; set; }
    }
}
