using Helium.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests
{
    public class HealthzTest
    {
        private readonly Mock<ILogger<HealthzController>> logger = new Mock<ILogger<HealthzController>>();
        private readonly HealthzController c;

        public HealthzTest()
        {
            c = new HealthzController(logger.Object, TestApp.MockDal);
        }

        [Fact]
        public void GetHealthz()
        {

            var res = (ObjectResult)c.Healthz();

            OkObjectResult ok = res as OkObjectResult;

            Assert.NotNull(ok);

            Helium.Model.HealthzSuccess z = ok.Value as Helium.Model.HealthzSuccess;

            Assert.NotNull(z);

            Assert.Equal(200, z.details.cosmosDb.details.Status);
            Assert.Equal(100, z.details.cosmosDb.details.Movies);
            Assert.Equal(531, z.details.cosmosDb.details.Actors);
            Assert.Equal(19, z.details.cosmosDb.details.Genres);
        }
    }
}
