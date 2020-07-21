using System;
using System.CommandLine;
using System.CommandLine.Parsing;
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
            string[] args;

            string keyvault = Environment.GetEnvironmentVariable("KEYVAULT_NAME");

            // test command line parser
            RootCommand root = App.BuildRootCommand();

            Assert.Empty(root.Parse("-k test-kv -a CLI").Errors);
            Assert.Empty(root.Parse("-k test-kv -a MSI").Errors);
            Assert.Empty(root.Parse("-k test-kv -a VS").Errors);

            Assert.Empty(root.Parse("-k test-kv -a CLI -d").Errors);
            Assert.Equal(1, root.Parse("-k test-kv -a CLI -h").Errors.Count);

            Assert.Equal(1, root.Parse("-k test-kv -a CLI -foo").Errors.Count);
            Assert.Equal(2, root.Parse("-k test-kv -a CLI -foo bar").Errors.Count);

            Assert.Equal(2, root.Parse("-k").Errors.Count);
            Assert.Equal(1, root.Parse("-k test-kv -a").Errors.Count);

            args = new string[] { "--help" };
            Assert.Equal(0, await App.Main(args));

            if (string.IsNullOrEmpty(keyvault))
            {
                Console.WriteLine("KEYVAULT_NAME not set");
            }
            else
            {
                args = new string[] { "-k", keyvault, "-a", "CLI", "-d" };
                Assert.Equal(0, await App.Main(args).ConfigureAwait(false));

                args = new string[] { "-k", keyvault, "-a", "CLI" };

                Console.WriteLine("Starting web server");
                App.Main(args).Wait(20000);
                Console.WriteLine("Web server stopped");

            }

            Console.WriteLine("AppTest Complete");
        }

    }
}
