using System.Linq;


namespace Helium.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB Interface
    /// </summary>
    public interface IDAL
    {
        System.Threading.Tasks.Task<Model.Actor> GetActorAsync(string actorId);
        IQueryable<Model.Actor> GetActors();
        IQueryable<Model.Actor> GetActorsByQuery(string q);
        IQueryable<string> GetGenres();
        System.Threading.Tasks.Task<Model.Movie> GetMovieAsync(string movieId);
        IQueryable<Model.Movie> GetMovies();
        IQueryable<Model.Movie> GetMoviesByQuery(string q, string genre = "", int year = 0, double rating = 0, bool toprated = false, string actorId = "");
        System.Collections.Generic.List<string> GetFeaturedMovieList();
        Helium.Model.HealthzSuccessDetails GetHealthz();
    }
}