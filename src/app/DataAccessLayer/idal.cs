using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB Interface
    /// </summary>
    public interface IDAL
    {
        Task<Model.Actor> GetActorAsync(string actorId);
        Task<IEnumerable<Model.Actor>> GetActorsAsync(string q, int offset = 0, int limit = 0);
        Task<IEnumerable<string>> GetGenresAsync();
        Task<Model.Movie> GetMovieAsync(string movieId);
        Task<IEnumerable<Model.Movie>> GetMoviesAsync(string q, string genre = "", int year = 0, double rating = 0, string actorId = "", int offset = 0, int limit = 0);
        Task<List<string>> GetFeaturedMovieListAsync();
        Task Reconnect(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, bool force = false);
    }
}