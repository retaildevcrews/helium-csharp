using Helium.DataAccessLayer;
using Helium.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests
{
    public sealed class TestApp
    {
        // static instance to prevent reloading

        public static MockDal MockDal { get; } = new MockDal();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
    public class MockDal : IDAL
    {
#pragma warning disable CA2227 // these can't be read-only
        public static List<Actor> Actors { get; set; }
        public static List<Movie> Movies { get; set; }
        public static List<string> Genres { get; set; }
#pragma warning restore CA2227

        public MockDal()
        {
            string path = "data/";

            if (!File.Exists(path + "actors.json"))
            {
                // support running from VS without copying data
                path = "../../../" + path;
            }

            if (!File.Exists(path + "actors.json") ||
                !File.Exists(path + "genres.json") ||
                !File.Exists(path + "movies.json"))
            {
                Console.WriteLine("Unable to find data files");
                Environment.Exit(-1);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
            };

            // load the data from the json files
            Actors = JsonConvert.DeserializeObject<List<Actor>>(File.ReadAllText(path + "actors.json"), settings);

            Movies = JsonConvert.DeserializeObject<List<Movie>>(File.ReadAllText(path + "movies.json"), settings);

            List<GenreDoc> list = JsonConvert.DeserializeObject<List<GenreDoc>>(File.ReadAllText(path + "genres.json"), settings);

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

        public Task<IEnumerable<Actor>> GetActorsAsync(int offset = 0, int limit = 0)
        {
            return Task<IEnumerable<Actor>>.Factory.StartNew(() => { return Actors; });
        }

        public Task<IEnumerable<Actor>> GetActorsByQueryAsync(string q, int offset = 0, int limit = 0)
        {
            // string.empty is valid, but null is not
            if (q == null)
            {
                throw new ArgumentNullException(nameof(q));
            }

            List<Actor> res = new List<Actor>();

            foreach (Actor a in Actors)
            {
                if (a.TextSearch.Contains(q.Trim(), StringComparison.OrdinalIgnoreCase))
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

        public Task<IEnumerable<Movie>> GetMoviesAsync(int offset = 0, int limit = 0)
        {
            return Task<IEnumerable<Movie>>.Factory.StartNew(() => { return Movies; });
        }

        public Task<IEnumerable<Movie>> GetMoviesByQueryAsync(string q, string genre, int year = 0, double rating = 0.0, string actorId = "", int offset = 0, int limit = 0)
        {
            List<Movie> res = new List<Movie>();

            if (!string.IsNullOrEmpty(q))
            {
                foreach (Movie m in Movies)
                {
                    if (m.TextSearch.Contains(q, StringComparison.OrdinalIgnoreCase))
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

            else if (!string.IsNullOrEmpty(actorId))
            {
#pragma warning disable CA1308 // lower is correct
                actorId = actorId.Trim().ToLowerInvariant();
#pragma warning restore CA1308 

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

        public Task Reconnect(Uri cosmosUrl, string cosmosKey, string cosmosDatabase, string cosmosCollection, bool force = false)
        {
            // do nothing
            return null;
        }
    }

    public class GenreDoc
    {
        public string Genre { get; set; }
    }
}
