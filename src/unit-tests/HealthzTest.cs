using Helium.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class HealthzTest
    {
        private readonly Mock<ILogger<HealthzController>> _logger = new Mock<ILogger<HealthzController>>();
        private readonly HealthzController _controller;

        public HealthzTest()
        {
            // TODO - need to mock IConfiguration
            //_controller = new HealthzController(_logger.Object, TestApp.MockDal);
        }

        [Fact]
        public async Task GetHealthz()
        {

            //var res = await _controller.HealthzAsync();

            //OkObjectResult ok = res as OkObjectResult;

            //Assert.NotNull(ok);

            //Helium.Model.HealthzSuccess z = ok.Value as Helium.Model.HealthzSuccess;

            //Assert.NotNull(z);

            //Assert.Equal(200, z.details.cosmosDb.details.Status);
            //Assert.Equal(100, z.details.cosmosDb.details.Movies);
            //Assert.Equal(531, z.details.cosmosDb.details.Actors);
            //Assert.Equal(19, z.details.cosmosDb.details.Genres);
        }
    }
}
