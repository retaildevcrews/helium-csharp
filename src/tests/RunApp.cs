using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CSE.Helium.Tests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
    public class AppTest
    {
        [Fact]
        public async Task RunApp()
        {
            string[] args = new string[] { "-k", "heliumtest-kv", "-a", "CLI", "-d" };

            int i = await CSE.Helium.App.Main(args).ConfigureAwait(false);
            Assert.Equal(0, i);

            args = new string[] { "-k", "heliumtest-kv", "-a", "CLI" };
            Task<int> t = CSE.Helium.App.Main(args);

            // wait for web server to start
            await Task.Delay(5000);


            using HttpClient client = new HttpClient { BaseAddress = new System.Uri("http://localhost:4120") };

            // will throw exceptions on failure so no need to assert
            await client.GetStringAsync("/healthz/dotnet");
            await client.GetStringAsync("/index.html");
            await client.GetStringAsync("/swagger/helium.json");
            await client.GetStringAsync("/version");
            await client.GetStringAsync("/robots.txt");
            await client.GetStringAsync("/api/genres");

            await client.GetStringAsync("/api/actors?pageNumber=2&pageSize=10");
            await client.GetStringAsync("/api/actors?q=nicole&pageNumber=1&pageSize=10");
            await client.GetStringAsync("/api/actors/nm0000173");
            await client.GetStringAsync("/api/actors/nm0000031");

            await client.GetStringAsync("/healthz");

            await client.GetStringAsync("/api/movies?pageNumber=2&pageSize=10");
            await client.GetStringAsync("/api/movies?q=ring&pageNumber=1&pageSize=10");
            await client.GetStringAsync("/api/featured/movie");
            await client.GetStringAsync("/api/movies/tt0133093");
            await client.GetStringAsync("/api/movies?actorId=nm0000206");
            await client.GetStringAsync("/api/movies?rating=8.5");
            await client.GetStringAsync("/api/movies?year=1999");
            await client.GetStringAsync("/api/movies?genre=action");
            await client.GetStringAsync("/healthz/ietf");

            await client.GetAsync("/api/movies?pageSize=-1");
            await client.GetAsync("/api/movies?pageSize=1001");
            await client.GetAsync("/api/movies?pageSize=10.1");
            await client.GetAsync("/api/movies?pageSize=foo");
            await client.GetAsync("/api/movies?pageNumber=0");
            await client.GetAsync("/api/movies?pageNumber=-1");
            await client.GetAsync("/api/movies?pageNumber=10.1");
            await client.GetAsync("/api/movies?pageNumber=10001");
            await client.GetAsync("/api/movies?pageNumber=foo");
            await client.GetAsync("/api/movies?year=foo");
            await client.GetAsync("/api/movies?year=-1");
            await client.GetAsync("/api/movies?year=0");
            await client.GetAsync("/api/movies?year=1");
            await client.GetAsync("/api/movies?year=1873");
            await client.GetAsync("/api/movies?year=2026");
            await client.GetAsync("/api/movies?year=2020.1");
            await client.GetAsync("/api/movies?year=2025");
            await client.GetAsync("/api/movies?year=1874");
            await client.GetAsync("/api/movies?rating=foo");
            await client.GetAsync("/api/movies?rating=-1");
            await client.GetAsync("/api/movies?rating=10.1");
            await client.GetAsync("/api/movies?genre=ab");
            await client.GetAsync("/api/movies?genre=123456789012345678901");
            await client.GetAsync("/api/movies?actorId=nm123");
            await client.GetAsync("/api/movies?actorId=ab12345");
            await client.GetAsync("/api/movies?actorId=tt12345");
            await client.GetAsync("/api/movies?actorId=NM12345");
            await client.GetAsync("/api/movies?actorId=nm12345");
            await client.GetAsync("/api/actors/ab12345");
            await client.GetAsync("/api/actors/tt12345");
            await client.GetAsync("/api/actors/NM12345");
            await client.GetAsync("/api/actors/nm123");
            await client.GetAsync("/api/actors/nmabcde");
            await client.GetAsync("/api/actors/nm00000");
            await client.GetAsync("/api/actors/nm12345");
            await client.GetAsync("/api/actors/nm0000173/foo");
            await client.GetAsync("/api/movies/ab12345");
            await client.GetAsync("/api/movies/nm12345");
            await client.GetAsync("/api/movies/TT12345");
            await client.GetAsync("/api/movies/tt123");
            await client.GetAsync("/api/movies/ttabcde");
            await client.GetAsync("/api/movies/tt00000");
            await client.GetAsync("/api/movies/tt12345");
            await client.GetAsync("/api/movies/tt0133093/foo");

            CSE.Helium.App.Stop();

            await Task.Delay(1000);

            Assert.True(t.IsCompletedSuccessfully);
        }

    }
}
