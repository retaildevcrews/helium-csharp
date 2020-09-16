using System;
using CSE.Helium.Model;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace CSE.Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        // select template for Actors
        const string actorSelect = "select m.id, m.partitionKey, m.actorId, m.type, m.name, m.birthYear, m.deathYear, m.profession, m.textSearch, m.movies from m where m.type = 'Actor' ";
        const string actorOrderBy = " order by m.textSearch ASC, m.actorId ASC";
        const string actorOffset = " offset {0} limit {1}";

        /// <summary>
        /// Retrieve a single Actor from CosmosDB by actorId
        /// 
        /// Uses the CosmosDB single document read API which is 1 RU if less than 1K doc size
        /// 
        /// Throws an exception if not found
        /// </summary>
        /// <param name="actorId">Actor ID</param>
        /// <returns>Actor object</returns>
        public async Task<Actor> GetActorAsync(string actorId)
        {
            // get the partition key for the actor ID
            // note: if the key cannot be determined from the ID, ReadDocumentAsync cannot be used.
            // ComputePartitionKey will throw an ArgumentException if the actorId isn't valid
            // get an actor by ID
            return await cosmosDetails.Container.ReadItemAsync<Actor>(actorId, new PartitionKey(Actor.ComputePartitionKey(actorId))).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a list of Actors by search string
        /// 
        /// The search is a "contains" search on actor name
        /// If q is empty, all actors are returned
        /// </summary>
        /// <param name="actorQueryParameters">search parameters</param>
        /// <returns>List of Actors or an empty list</returns>
        public async Task<IEnumerable<Actor>> GetActorsAsync(ActorQueryParameters actorQueryParameters)
        {
            _ = actorQueryParameters ?? throw new ArgumentNullException(nameof(actorQueryParameters));

            string sql = actorSelect;

            int offset = actorQueryParameters.PageSize * actorQueryParameters.GetZeroBasedPageNumber();
            int limit = actorQueryParameters.PageSize;

            string offsetLimit = string.Format(CultureInfo.InvariantCulture, actorOffset, offset, limit);

            if (!string.IsNullOrEmpty(actorQueryParameters.Q))
            {
                // convert to lower and escape embedded '
                actorQueryParameters.Q = actorQueryParameters.Q.Trim().ToLowerInvariant().Replace("'", "''", System.StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(actorQueryParameters.Q))
                {
                    // get actors by a "like" search on name
                    sql += string.Format(CultureInfo.InvariantCulture, $" and contains(m.textSearch, @q) ");
                }
            }

            sql += actorOrderBy + offsetLimit;

            QueryDefinition queryDefinition = new QueryDefinition(sql);

            if (!string.IsNullOrEmpty(actorQueryParameters.Q))
            {
                queryDefinition.WithParameter("@q", actorQueryParameters.Q);
            }

            return await InternalCosmosDBSqlQuery<Actor>(queryDefinition).ConfigureAwait(false);
        }

    }
}