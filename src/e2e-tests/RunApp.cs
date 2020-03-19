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
        [Fact]
        public async Task RunApp()
        {
            string[] args = new string[] { "--kvname", "bluebell-kv", "--authtype", "CLI"  };

            Task<int> t = Helium.App.Main(args);

            await Task.Delay(10000);

            string res;

            using HttpClient client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:4120");

            try
            {
                res = await client.GetStringAsync("/healthz/dotnet");
                res = await client.GetStringAsync("/index.html");
                res = await client.GetStringAsync("/swagger.json");
                res = await client.GetStringAsync("/version");
                res = await client.GetStringAsync("/robots.txt");
                res = await client.GetStringAsync("/api/genres");

                res = await client.GetStringAsync("/api/actors");
                res = await client.GetStringAsync("/api/actors?q=nicole");
                res = await client.GetStringAsync("/api/actors/nm0000173");

                res = await client.GetStringAsync("/healthz");

                res = await client.GetStringAsync("/api/movies");
                res = await client.GetStringAsync("/api/featured/movie");
                res = await client.GetStringAsync("/api/movies/tt0133093");
                res = await client.GetStringAsync("/api/movies?actorId=nm0000206");
                res = await client.GetStringAsync("/api/movies?rating=8.5");
                res = await client.GetStringAsync("/api/movies?q=ring");
                res = await client.GetStringAsync("/api/movies?year=1999");
                res = await client.GetStringAsync("/api/movies?genre=action");
                res = await client.GetStringAsync("/healthz/ietf");
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
