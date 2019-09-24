using Helium;
using Helium.Controllers;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using Xunit;

namespace UnitTests
{
    public class ActorsTest
    {
        private readonly Mock<ILogger<ActorsController>> logger = new Mock<ILogger<ActorsController>>();
        private readonly ActorsController c;

        public ActorsTest()
        {
            c = new ActorsController(logger.Object, TestApp.MockDal);
        }

        [Fact]
        public void GetAllActors()
        {
            var l = c.GetActors(string.Empty).ToList();

            Assert.Equal(AssertValues.ActorsCount, l.Count);

        }

        [Fact]
        public void GetActorsBySearch()
        {
            var l = c.GetActors(AssertValues.ActorSearchString).ToList();

            Assert.Equal(AssertValues.ActorsSearchCount, l.Count);

        }

        [Fact]
        public async void GetActorByIdPass()
        {
            // do not use c.GetActorByIdAsync().Result
            // due to thread syncronization issues with build clients, it is not reliable
            var res = await c.GetActorByIdAsync(AssertValues.ActorById);
            OkObjectResult ok = res as OkObjectResult;

            Assert.NotNull(ok);

            Actor actor = ok.Value as Actor;

            Assert.NotNull(actor);
            Assert.Equal(Helium.DataAccessLayer.DAL.GetPartitionKey(actor.ActorId), actor.PartitionKey);
            Assert.Equal(AssertValues.ActorByIdName, actor.Name);
        }

        [Fact]
        public async void GetActorByIdFail()
        {
            var res = await c.GetActorByIdAsync(AssertValues.BadId);

            NotFoundResult nf = res as NotFoundResult;

            Assert.NotNull(nf);
            Assert.Equal(Constants.NotFound, nf.StatusCode);
        }
    }
}
