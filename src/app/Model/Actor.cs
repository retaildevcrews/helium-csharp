using System.Collections.Generic;
using System;
using System.Globalization;

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

        /// <summary>
        /// Compute the partition key based on the movieId or actorId
        /// 
        /// For this sample, the partitionkey is the id mod 10
        /// 
        /// In a full implementation, you would update the logic to determine the partition key
        /// </summary>
        /// <param name="id">document id</param>
        /// <returns>the partition key</returns>
        public static string ComputePartitionKey(string id)
        {
            // validate id
            if (!string.IsNullOrEmpty(id) &&
                id.Length > 5 &&
                id.StartsWith("nm", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(id.Substring(2), out int idInt))
            {
                return (idInt % 10).ToString(CultureInfo.InvariantCulture);
            }

            throw new ArgumentException("Invalid Partition Key");
        }
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
