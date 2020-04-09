using Helium.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class DAL
    {
        // select template for Actors
        const string _actorSelect = "select m.id, m.partitionKey, m.actorId, m.type, m.name, m.birthYear, m.deathYear, m.profession, m.textSearch, m.movies from m where m.type = 'Actor' ";
        const string _actorOrderBy = " order by m.textSearch ASC, m.actorId ASC";
        const string _actorOffset = " offset @offset limit @limit";

        /// <summary>
        /// Retrieve a single Actor from CosmosDB by actorId
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
            return await _cosmosDetails.Container.ReadItemAsync<Actor>(actorId, new PartitionKey(GetPartitionKey(actorId))).ConfigureAwait(false);
        }

        ///// <summary>
        ///// Parameterize Actors to avoid sql injection attacks
        ///// </summary>
        ///// <param name="q">search term</param>
        ///// <param name="parameters">parameter check</param>
        ///// <returns>parameterized results</returns>
        //public async Task<IEnumerable<Actor>> GetMyEntriesAsync(string q, Dictionary<string, object> parameters)
        //{
        //    if ((parameters?.Count ?? 0) < 1)
        //    {
        //        throw new ArgumentException("Parameters required to prevent SQL injection attacks");
        //    }

        //    var queryDef = new QueryDefinition(q);
        //    foreach (var p in parameters)
        //    {
        //        queryDef.WithParameter(p.Key, p.Value);
        //    }

        //    var query = this._cosmosDetails.Container.GetItemQueryIterator<Actor>(queryDef);
        //    List<Actor> results = new List<Actor>();
        //    while (query.HasMoreResults)
        //    {
        //        var response = await query.ReadNextAsync().ConfigureAwait(false);
        //        results.AddRange(response.ToList());
        //    }

        //    return results;
        //}

        /// <summary>
        /// Get a list of Actors by search string
        /// 
        /// The search is a "contains" search on actor name
        /// If q is empty, all actors are returned
        /// </summary>
        /// <param name="q">search term</param>
        /// <param name="offset">zero based offset for paging</param>
        /// <param name="limit">number of documents for paging</param>
        /// <returns>List of Actors or an empty list</returns>
        public async Task<IEnumerable<Actor>> GetActorsAsync(string q, int offset = 0, int limit = Constants.DefaultPageSize)
        {
            string sql = _actorSelect;
            string orderby = _actorOrderBy;

            Dictionary<string, string> queryParams = new Dictionary<string, string>();

            if (limit < 1)
            {
                limit = Constants.DefaultPageSize;
            }
            else if (limit > Constants.MaxPageSize)
            {
                limit = Constants.MaxPageSize;
            }

            // string offsetLimit = string.Format(CultureInfo.InvariantCulture, _actorOffset, offset, limit);

            queryParams.Add("@offset", offset.ToString());
            queryParams.Add("@limit", limit.ToString());

            if (!string.IsNullOrEmpty(q))
            {
                // convert to lower and escape embedded '
                q = q.Trim().ToLowerInvariant().Replace("'", "''", System.StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(q))
                {
                    // get actors by a "like" search on name
                    //sql += string.Format(CultureInfo.InvariantCulture, $" and contains(m.textSearch, '{q}') ");
                    sql += " and contains(m.textSearch, @q) ";
                    queryParams.Add("@q", q);
                }
            }

            sql += orderby + _actorOffset;

            QueryDefinition queryDef = new QueryDefinition(sql);
            if(queryParams.Count > 0)
            {
                foreach(string param in queryParams.Keys)
                {
                    queryDef.WithParameter(param, queryParams[param]);
                }
            }

            return await InternalCosmosDBSqlQuery<Actor>(queryDef).ConfigureAwait(false);
        }

    }

}