using Helium;
using Helium.Controllers;
using Helium.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
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
            OkObjectResult ok = c.GetActors(string.Empty) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Actor>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.ActorsCount, ie.ToList<Actor>().Count);

        }

        [Fact]
        public void GetActorsBySearch()
        {
            OkObjectResult ok = c.GetActors(AssertValues.ActorSearchString) as OkObjectResult;

            Assert.NotNull(ok);

            var ie = ok.Value as IEnumerable<Actor>;

            Assert.NotNull(ie);

            Assert.Equal(AssertValues.ActorsSearchCount, ie.ToList<Actor>().Count);

        }

        [Fact]
        public async void GetActorByIdPass()
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
        public async void GetActorByIdFail()
        {
            // this will fail GetPartitionKey
            NotFoundResult nf = await c.GetActorByIdAsync(AssertValues.BadId) as NotFoundResult;

            Assert.NotNull(nf);
            Assert.Equal(Constants.NotFound, nf.StatusCode);

            // this will fail search
            nf = await c.GetActorByIdAsync(AssertValues.ActorById + "000") as NotFoundResult;

            Assert.NotNull(nf);
            Assert.Equal(Constants.NotFound, nf.StatusCode);
        }
    }
}
