using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSE.Helium.Model;

namespace CSE.Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB Interface
    /// </summary>
    public interface IDAL
    {
        Task<Actor> GetActorAsync(string actorId);
        Task<IEnumerable<Actor>> GetActorsAsync(ActorQueryParameters actorQueryParameters);
        Task<IEnumerable<string>> GetGenresAsync();
        Task<Movie> GetMovieAsync(string movieId);
        Task<IEnumerable<Movie>> GetMoviesAsync(MovieQueryParameters movieQueryParameters);
        Task<List<string>> GetFeaturedMovieListAsync();
        Task Reconnect(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, bool force = false);
    }
}