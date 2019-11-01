using Helium.DataAccessLayer;
using Helium.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests
{
    public class TestApp
    {
        // static instance to prevent reloading

        public static MockDal MockDal { get; } = new MockDal();
    }

    public class MockDal : IDAL
    {
        public static List<Actor> Actors;
        public static List<Movie> Movies;
        public static List<string> Genres;

        public MockDal()
        {
            string path = "../data/";

            if (!File.Exists(path + "actors.json"))
            {
                path = "../../../" + path;
            }

            if (!File.Exists(path + "actors.json") ||
                !File.Exists(path + "genres.json") ||
                !File.Exists(path + "movies.json"))
            {
                Console.WriteLine("Unable to find data files");
                Environment.Exit(-1);
            }

            // load the data from the json files
            Actors = JsonConvert.DeserializeObject<List<Actor>>(File.ReadAllText(path + "actors.json"));

            Movies = JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText(path + "movies.json"));

            List<GenreDoc> list = JsonConvert.DeserializeObject<List<GenreDoc>>(File.ReadAllText(path + "genres.json"));

            Genres = new List<string>();

            foreach (GenreDoc g in list)
            {
                Genres.Add(g.Genre);
            }
        }

        public Task<Actor> GetActorAsync(string actorId)
        {
            string pk = Helium.DataAccessLayer.DAL.GetPartitionKey(actorId);

            foreach (Actor a in Actors)
            {
                if (a.ActorId == actorId)
                {
                    return Task<Actor>.Factory.StartNew(() => { return a; });
                }
            }

            throw new ArgumentException("NotFound");
        }

        public Task<IEnumerable<Actor>> GetActorsAsync()
        {
            return Task<IEnumerable<Actor>>.Factory.StartNew(() => { return Actors; });
        }

        public Task<IEnumerable<Actor>> GetActorsByQueryAsync(string q)
        {
            List<Actor> res = new List<Actor>();

            q = q.ToLower().Trim();

            foreach (Actor a in Actors)
            {
                if (a.TextSearch.Contains(q))
                {
                    res.Add(a);
                }
            }

            return Task<IEnumerable<Actor>>.Factory.StartNew(() => { return res; });
        }

        public Task<IEnumerable<string>> GetGenresAsync()
        {
            return Task<IEnumerable<string>>.Factory.StartNew(() => { return Genres; });
        }

        public Task<Movie> GetMovieAsync(string movieId)
        {
            string pk = Helium.DataAccessLayer.DAL.GetPartitionKey(movieId);

            foreach (Movie m in Movies)
            {
                if (m.MovieId == movieId)
                {
                    return Task<Movie>.Factory.StartNew(() => { return m; });
                }
            }

            throw new ArgumentException("NotFound");
        }

        public Task<IEnumerable<Movie>> GetMoviesAsync()
        {
            return Task<IEnumerable<Movie>>.Factory.StartNew(() => { return Movies; });
        }

        public Task<IEnumerable<Movie>> GetMoviesByQueryAsync(string q, string genre, int year = 0, double rating = 0.0, bool topRated = false, string actorId = "")
        {
            List<Movie> res = new List<Movie>();

            if (!string.IsNullOrEmpty(q))
            {
                q = q.ToLower();

                foreach (Movie m in Movies)
                {
                    if (m.TextSearch.Contains(q))
                    {
                        res.Add(m);
                    }
                }
            }

            else if (!string.IsNullOrEmpty(genre))
            {
                foreach (Movie m in Movies)
                {
                    if (m.Genres.Contains(genre, StringComparer.OrdinalIgnoreCase))
                    {
                        res.Add(m);
                    }
                }
            }

            else if (year > 0)
            {
                foreach (Movie m in Movies)
                {
                    if (m.Year == year)
                    {
                        res.Add(m);
                    }
                }
            }

            else if (rating > 0)
            {
                foreach (Movie m in Movies)
                {
                    if (m.Rating >= rating)
                    {
                        res.Add(m);
                    }
                }
            }

            else if (topRated)
            {
                foreach (Movie m in Movies)
                {
                    if (m.Rating >= 8.8)
                    {
                        res.Add(m);
                    }
                }
            }

            else if (!string.IsNullOrEmpty(actorId))
            {
                actorId = actorId.Trim().ToLower();

                foreach (Movie m in Movies)
                {
                    foreach (var a in m.Roles)
                    {
                        if (a.ActorId == actorId)
                        {
                            res.Add(m);
                        }
                    }
                }
            }

            else
            {
                return Task<IEnumerable<Movie>>.Factory.StartNew(() => { return Movies; });
            }

            return Task<IEnumerable<Movie>>.Factory.StartNew(() => { return res; });
        }

        public Task<List<string>> GetFeaturedMovieListAsync()
        {
            List<string> res = new List<string> { "tt0133093", "tt0120737", "tt0167260", "tt0167261", "tt0372784", "tt0172495", "tt0317705" };

            return Task<List<string>>.Factory.StartNew(() => { return res; });
        }

        public Task<HealthzSuccessDetails> GetHealthzAsync()
        {
            HealthzSuccessDetails d = new HealthzSuccessDetails();

            d.Actors = 531;
            d.Movies = 100;
            d.Genres = 19;

            return Task<HealthzSuccessDetails>.Factory.StartNew(() => { return d; });
        }

        public Task Reconnect(string cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, bool force = false)
        {
            // do nothing
            return null;
        }
    }

    public class GenreDoc
    {
        public string Genre;
    }
}
