using Helium;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class ParamTest
    {
        [Fact]
        public async Task AppMainTest ()
        {
            string[] args = Array.Empty<string>();

            int i = await App.Main(args);

            Assert.NotEqual(0, i);

            args = new string[] { "--help" };
            i = await App.Main(args);
            Assert.Equal(0, i);

            args = new string[] { "-k", "heliumtest-kv", "-a", "FOO" };
            i = await App.Main(args);
            Assert.NotEqual(0, i);

            Assert.Equal(4, App.CombineEnvVarsWithCommandLine(null).Count);

            Environment.SetEnvironmentVariable("KEYVAULT_NAME", "heliumtest-kv");
            Environment.SetEnvironmentVariable("AUTH_TYPE", "CLI");
            Environment.SetEnvironmentVariable("LOG_LEVEL", "Information");

            List<string> cmd = App.CombineEnvVarsWithCommandLine(Array.Empty<string>());
            Assert.Contains("--keyvault-name", cmd);
            Assert.Contains("heliumtest-kv", cmd);
            Assert.Contains("--auth-type", cmd);
            Assert.Contains("CLI", cmd);
            Assert.Contains("--log-level", cmd);
            Assert.Contains("Information", cmd);

            Environment.SetEnvironmentVariable("KEYVAULT_NAME", null);
            Environment.SetEnvironmentVariable("AUTH_TYPE", null);
            Environment.SetEnvironmentVariable("LOG_LEVEL", null);
        }

        [Fact]
        public void CommandLineTest()
        {
            RootCommand root = App.BuildRootCommand();

            Assert.Empty(root.Parse("-k heliumtest-kv -a CLI").Errors);
            Assert.Empty(root.Parse("-k heliumtest-kv -a MSI").Errors);
            Assert.Empty(root.Parse("-k heliumtest-kv -a VS").Errors);

            Assert.Empty(root.Parse("-k heliumtest-kv -a CLI -d").Errors);
            Assert.Equal(1, root.Parse("-k heliumtest-kv -a CLI -h").Errors.Count);

            Assert.Equal(1, root.Parse("-k heliumtest-kv -a CLI -foo").Errors.Count);
            Assert.Equal(2, root.Parse("-k heliumtest-kv -a CLI -foo bar").Errors.Count);

            Assert.Equal(2, root.Parse("-k").Errors.Count);
            Assert.Equal(1, root.Parse("-k heliumtest-kv -a").Errors.Count);
        }
    }
}
