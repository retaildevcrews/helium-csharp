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

        public IQueryable<Actor> GetActors()
        {
            return Actors.AsQueryable();
        }

        public IQueryable<Actor> GetActorsByQuery(string q)
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

            return res.AsQueryable();
        }

        public IQueryable<string> GetGenres()
        {
            return Genres.AsQueryable<string>();
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

        public IQueryable<Movie> GetMovies()
        {
            return Movies.AsQueryable();
        }

        public IQueryable<Movie> GetMoviesByQuery(string q)
        {
            List<Movie> res = new List<Movie>();

            q = q.ToLower();

            foreach (Movie m in Movies)
            {
                if (m.TextSearch.Contains(q))
                {
                    res.Add(m);
                }
            }

            return res.AsQueryable();
        }

        public HealthzSuccessDetails GetHealthz()
        {
            HealthzSuccessDetails d = new HealthzSuccessDetails();

            d.Actors = 531;
            d.Movies = 100;
            d.Genres = 19;

            return d;
        }
    }


    public class GenreDoc
    {
        public string Genre;
    }
}
