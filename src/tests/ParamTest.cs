using Helium;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    // TODO - fix after refactor is complete

    public class ParamTest
    {
        [Fact]
        public async Task NoParamsTest()
        {
            string[] args = Array.Empty<string>();

            int i = await App.Main(args);

            Assert.Equal(1, i);

            args = new string[] { "--help" };
            i = await App.Main(args);
            Assert.Equal(0, i);

            args = new string[] { "-k", "heliumtest-kv", "-a", "FOO" };
            i = await App.Main(args);
            Assert.Equal(-1, i);

            Assert.Empty(App.GetCommandString(null));

            Environment.SetEnvironmentVariable("KEYVAULT_NAME", "heliumtest-kv");
            Environment.SetEnvironmentVariable("AUTH_TYPE", "CLI");

            Assert.Equal(" --keyvault-name heliumtest-kv --auth-type CLI", App.GetCommandString(Array.Empty<string>()));

            Environment.SetEnvironmentVariable("KEYVAULT_NAME", null);
            Environment.SetEnvironmentVariable("AUTH_TYPE", null);
        }

        [Fact]
        public void CommandLineTest()
        {
            ParseResult parse;
            string cmd;

            RootCommand root = App.BuildRootCommand();

            Assert.Empty(root.Parse("-k heliumtest-kv -a CLI").Errors);
            Assert.Empty(root.Parse("-k heliumtest-kv -a MSI").Errors);
            Assert.Empty(root.Parse("-k heliumtest-kv -a VS").Errors);

            Assert.Empty(root.Parse("-k heliumtest-kv -a CLI -d").Errors);
            Assert.Equal(1, root.Parse("-k heliumtest-kv -a CLI -h").Errors.Count);

            Assert.Equal(1, root.Parse("-k heliumtest-kv -a CLI -foo").Errors.Count);
            Assert.Equal(2, root.Parse("-k heliumtest-kv -a CLI -foo bar").Errors.Count);

            Assert.Equal(1, root.Parse("-k").Errors.Count);
            Assert.Equal(1, root.Parse("-k heliumtest-kv -a").Errors.Count);


            parse = root.Parse("-k heliumtest-kv -a MSI");

        }

        //[Fact]
        //public void EnvVarValuesTest()
        //{
        //    string[] args = Array.Empty<string>();

        //    // test kvname env var
        //    bool flag = App.ProcessArgs(args, out string kvUrl, out string authType, out bool helpFlag);
        //    Assert.True(flag);
        //    Assert.False(helpFlag);
        //    Assert.Equal("https://testkv.vault.azure.net/", kvUrl);
        //    Assert.Equal("MSI", authType);

        //    // test both env var
        //    flag = App.ProcessArgs(args, out kvUrl, out authType, out helpFlag);
        //    Assert.True(flag);
        //    Assert.False(helpFlag);
        //    Assert.Equal("https://testkv.vault.azure.net/", kvUrl);
        //    Assert.Equal("CLI", authType);

        //    // test bad auth type
        //    Environment.SetEnvironmentVariable("AUTH_TYPE", "foo");
        //    flag = App.ProcessArgs(args, out kvUrl, out authType, out helpFlag);
        //    Assert.False(flag);
        //    Assert.False(helpFlag);
        //    Assert.Null(kvUrl);
        //    Assert.Null(authType);

        //    // test cmd line overriding env var
        //    Environment.SetEnvironmentVariable("AUTH_TYPE", "CLI");
        //    args = new string[] { "--kvname", "testkv2", "--authtype", "VS" };
        //    flag = App.ProcessArgs(args, out kvUrl, out authType, out helpFlag);
        //    Assert.True(flag);
        //    Assert.False(helpFlag);
        //    Assert.Equal("https://testkv2.vault.azure.net/", kvUrl);
        //    Assert.Equal("VS", authType);

        //}
    }
}
