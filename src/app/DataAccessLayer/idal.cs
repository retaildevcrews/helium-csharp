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
        Task<IEnumerable<Model.Actor>> GetActorsAsync(int offset = 0, int limit = 0);
        Task<IEnumerable<Model.Actor>> GetActorsByQueryAsync(string q, int offset = 0, int limit = 0);
        Task<IEnumerable<string>> GetGenresAsync();
        Task<Model.Movie> GetMovieAsync(string movieId);
        Task<IEnumerable<Model.Movie>> GetMoviesAsync(int offset = 0, int limit = 0);
        Task<IEnumerable<Model.Movie>> GetMoviesByQueryAsync(string q, string genre = "", int year = 0, double rating = 0, bool toprated = false, string actorId = "", int offset = 0, int limit = 0);
        Task<List<string>> GetFeaturedMovieListAsync();
        Task<Helium.Model.HealthzSuccessDetails> GetHealthzAsync();
        Task Reconnect(string cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, bool force = false);
    }
}