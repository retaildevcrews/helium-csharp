using Helium.Model;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        // select template for Actors
        const string actorSelect = "select m.id, m.partitionKey, m.actorId, m.type, m.name, m.birthYear, m.deathYear, m.profession, m.textSearch, m.movies from m where m.type = 'Actor' ";

        /// <summary>
        /// Retrieve a single actor from CosmosDB by actorId
        /// 
        /// Uses the CosmosDB single document read API which is 1 RU if less than 1K doc size
        /// 
        /// Throws an exception if not found
        /// </summary>
        /// <param name="actorId">Actor ID</param>
        /// <returns>Actor object</returns>
        public async System.Threading.Tasks.Task<Actor> GetActorAsync(string actorId)
        {
            // get the partition key for the actor ID
            // note: if the key cannot be determined from the ID, ReadDocumentAsync cannot be used.
            // GetPartitionKey will throw an ArgumentException if the actorId isn't valid
            // get an actor by ID
            return await container.ReadItemAsync<Actor>(actorId, new PartitionKey(GetPartitionKey(actorId)));
        }

        /// <summary>
        /// Get all Actors from CosmosDB
        /// </summary>
        /// <returns>List of Actors</returns>
        public async Task<IEnumerable<Actor>> GetActorsAsync()
        {
            // get all actors
            return await GetActorsByQueryAsync(string.Empty);
        }

        /// <summary>
        /// Get Actors by search string
        /// 
        /// The search is a "contains" search on actor name
        /// If q is empty, all actors are returned
        /// </summary>
        /// <param name="q">search term</param>
        /// <returns>a list of Actors or an empty list</returns>
        public async Task<IEnumerable<Actor>> GetActorsByQueryAsync(string q)
        {
            string sql = actorSelect;

            if (!string.IsNullOrEmpty(q))
            {
                // convert to lower and escape embedded '
                q = q.Trim().ToLower().Replace("'", "''");

                if (!string.IsNullOrEmpty(q))
                {
                    // get actors by a "like" search on name
                    sql += string.Format(" and contains(m.textSearch, '{0}') ", q);
                }
            }

            sql += " order by m.name";

            return await QueryActorWorkerAsync(sql);
        }

        /// <summary>
        /// Actor Worker Query
        /// </summary>
        /// <param name="sql">select statement to execute</param>
        /// <returns>List of Actors</returns>
        public async Task<IEnumerable<Actor>> QueryActorWorkerAsync(string sql)
        {
            // run query
            var query = container.GetItemQueryIterator<Actor>(sql, requestOptions: queryRequestOptions);

            List<Actor> results = new List<Actor>();

            while (query.HasMoreResults)
            {
                foreach (var doc in await query.ReadNextAsync())
                {
                    results.Add(doc);
                }
            }
            return results;
        }
    }
}