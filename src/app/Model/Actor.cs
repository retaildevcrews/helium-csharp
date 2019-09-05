using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helium.Model
{
    public class ActorBase
    {
        public string ActorId;
        public string Name;
        public int BirthYear;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int DeathYear;
        public List<string> Profession;
    }

    public class Actor : ActorBase
    {
        [JsonProperty(Order = 97)]
        public string Type;
        [JsonProperty(Order = 98)]
        public string TextSearch;
        [JsonProperty(Order = 99)]
        public List<MovieBase> Movies;
    }
}
