using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helium.Model
{
    public class MovieBase
    {
        public string MovieId;
        public string Type;
        public string TextSearch;
        public string Title;
        public int Year;
        public int Runtime;
        public double Rating;
        public int Votes;
        public List<string> Genres;
    }

    public class Movie : MovieBase
    {
        [JsonProperty(Order = 99)]
        public List<Role> Roles;
    }

    public class Role : ActorBase
    {
        [JsonProperty(Order = 97)]
        public int Order;
        [JsonProperty(Order = 98)]
        public string Category;
        [JsonProperty(Order = 99)]
        public List<string> Characters;
    }
}
