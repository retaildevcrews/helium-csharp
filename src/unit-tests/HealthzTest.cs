using Helium;
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

            string s = ok.Value.ToString();

            Assert.Equal(Constants.HealthzResult, s);

        }
    }
}
