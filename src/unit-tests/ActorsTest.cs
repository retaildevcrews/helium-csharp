using Helium.Controllers;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task GetAllActors()
        {
            OkObjectResult ok = await c.GetActorsAsync(string.Empty) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Actor>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.ActorsCount, ie.ToList<Actor>().Count);
        }

        [Fact]
        public async Task GetActorsBySearch()
        {
            OkObjectResult ok = await c.GetActorsAsync(AssertValues.ActorSearchString) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Actor>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.ActorsSearchCount, ie.ToList<Actor>().Count);
        }

        [Fact]
        public async Task GetActorByIdPass()
        {
            // do not use c.GetActorByIdAsync().Result
            // due to thread syncronization issues with build clients, it is not reliable
            OkObjectResult ok = await c.GetActorByIdAsync(AssertValues.ActorById) as OkObjectResult;

            Assert.NotNull(ok);

            Actor actor = ok.Value as Actor;

            Assert.NotNull(actor);
            Assert.Equal(Helium.DataAccessLayer.DAL.GetPartitionKey(actor.ActorId), actor.PartitionKey);
            Assert.Equal(AssertValues.ActorByIdName, actor.Name);
        }

        [Fact]
        public async Task GetActorByIdFail()
        {
            // this will fail GetPartitionKey
            NotFoundResult nf = await c.GetActorByIdAsync(AssertValues.BadId) as NotFoundResult;

            Assert.NotNull(nf);
            Assert.Equal((int)System.Net.HttpStatusCode.NotFound, nf.StatusCode);

            // this will fail search
            nf = await c.GetActorByIdAsync(AssertValues.ActorById + "000") as NotFoundResult;

            Assert.NotNull(nf);
            Assert.Equal((int)System.Net.HttpStatusCode.NotFound, nf.StatusCode);
        }
    }
}
