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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable")]
    public class AppTest
    {
        [Fact]
        public void RunApp()
        {
            string[] args = new string[] { "--kvname", "bluebell-kv", "--authtype", "CLI"  };

            Helium.App.Main(args).Wait(30000);
        }

    }
}
