using Helium.Controllers;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
    public class AppTest
    {
        HttpResponseMessage resp;
        string res;

        [Fact]
        public async Task RunApp()
        {
            string[] args = new string[] { "--kvname", "heliumtest-kv", "--authtype", "CLI"  };

            await Helium.App.Main(null);
            await Helium.App.Main(new string[] { "--help" });
            await Helium.App.Main(new string[] { "--kvname", " " });

            Task<int> t = Helium.App.Main(args);

            await Task.Delay(10000);


            using HttpClient client = new HttpClient { BaseAddress = new System.Uri("http://localhost:4120") };

            try
            {
                res = await client.GetStringAsync("/healthz/dotnet");
                res = await client.GetStringAsync("/index.html");
                res = await client.GetStringAsync("/swagger.json");
                res = await client.GetStringAsync("/version");
                res = await client.GetStringAsync("/robots.txt");
                res = await client.GetStringAsync("/api/genres");

                res = await client.GetStringAsync("/api/actors?pageNumber=2&pageSize=10");
                res = await client.GetStringAsync("/api/actors?q=nicole&pageNumber=1&pageSize=10");
                res = await client.GetStringAsync("/api/actors/nm0000173");
                res = await client.GetStringAsync("/api/actors/nm0000031");

                res = await client.GetStringAsync("/healthz");

                res = await client.GetStringAsync("/api/movies?pageNumber=2&pageSize=10");
                res = await client.GetStringAsync("/api/movies?q=ring&pageNumber=1&pageSize=10");
                res = await client.GetStringAsync("/api/featured/movie");
                res = await client.GetStringAsync("/api/movies/tt0133093");
                res = await client.GetStringAsync("/api/movies?actorId=nm0000206");
                res = await client.GetStringAsync("/api/movies?rating=8.5");
                res = await client.GetStringAsync("/api/movies?year=1999");
                res = await client.GetStringAsync("/api/movies?genre=action");
                res = await client.GetStringAsync("/healthz/ietf");

                resp = await client.GetAsync("/api/movies?pageSize=-1");
                resp = await client.GetAsync("/api/movies?pageSize=1001");
                resp = await client.GetAsync("/api/movies?pageSize=10.1");
                resp = await client.GetAsync("/api/movies?pageSize=foo");
                resp = await client.GetAsync("/api/movies?pageNumber=0");
                resp = await client.GetAsync("/api/movies?pageNumber=-1");
                resp = await client.GetAsync("/api/movies?pageNumber=10.1");
                resp = await client.GetAsync("/api/movies?pageNumber=10001");
                resp = await client.GetAsync("/api/movies?pageNumber=foo");
                resp = await client.GetAsync("/api/movies?year=foo");
                resp = await client.GetAsync("/api/movies?year=-1");
                resp = await client.GetAsync("/api/movies?year=0");
                resp = await client.GetAsync("/api/movies?year=1");
                resp = await client.GetAsync("/api/movies?year=1873");
                resp = await client.GetAsync("/api/movies?year=2026");
                resp = await client.GetAsync("/api/movies?year=2020.1");
                resp = await client.GetAsync("/api/movies?year=2025");
                resp = await client.GetAsync("/api/movies?year=1874");
                resp = await client.GetAsync("/api/movies?rating=foo");
                resp = await client.GetAsync("/api/movies?rating=-1");
                resp = await client.GetAsync("/api/movies?rating=10.1");
                resp = await client.GetAsync("/api/movies?genre=ab");
                resp = await client.GetAsync("/api/movies?genre=123456789012345678901");
                resp = await client.GetAsync("/api/movies?actorId=nm123");
                resp = await client.GetAsync("/api/movies?actorId=ab12345");
                resp = await client.GetAsync("/api/movies?actorId=tt12345");
                resp = await client.GetAsync("/api/movies?actorId=NM12345");
                resp = await client.GetAsync("/api/movies?actorId=nm12345");
                resp = await client.GetAsync("/api/actors/ab12345");
                resp = await client.GetAsync("/api/actors/tt12345");
                resp = await client.GetAsync("/api/actors/NM12345");
                resp = await client.GetAsync("/api/actors/nm123");
                resp = await client.GetAsync("/api/actors/nmabcde");
                resp = await client.GetAsync("/api/actors/nm00000");
                resp = await client.GetAsync("/api/actors/nm12345");
                resp = await client.GetAsync("/api/actors/nm0000173/foo");
                resp = await client.GetAsync("/api/movies/ab12345");
                resp = await client.GetAsync("/api/movies/nm12345");
                resp = await client.GetAsync("/api/movies/TT12345");
                resp = await client.GetAsync("/api/movies/tt123");
                resp = await client.GetAsync("/api/movies/ttabcde");
                resp = await client.GetAsync("/api/movies/tt00000");
                resp = await client.GetAsync("/api/movies/tt12345");
                resp = await client.GetAsync("/api/movies/tt0133093/foo");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Helium.App.Stop();

            await Task.Delay(500);

        }

    }
}
