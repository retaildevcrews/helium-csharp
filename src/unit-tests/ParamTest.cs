using System;
using Helium;
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
    public class ParamTest
    {
        [Fact]
        public void NoParams()
        {
            string[] args = Array.Empty<string>();

            bool flag = App.ProcessArgs(args, out string kvUrl, out string authType, out bool helpFlag);

            Assert.True(flag);
            Assert.True(helpFlag);
            Assert.Null(kvUrl);
            Assert.Null(authType);
        }

        [Fact]
        public void HelpParamTest()
        {
            string[] args = Array.Empty<string>();

            bool flag = App.ProcessArgs(args, out string kvUrl, out string authType, out bool helpFlag);
            Assert.True(flag);
            Assert.True(helpFlag);
            Assert.Null(kvUrl);
            Assert.Null(authType);

            args = new string[] { "-h" };
            flag = App.ProcessArgs(args, out kvUrl, out authType, out helpFlag);
            Assert.True(flag);
            Assert.True(helpFlag);
            Assert.Null(kvUrl);
            Assert.Null(authType);
        }

        [Fact]
        public void KeyVaultNameTest()
        {
            string[] args = new string[] { "--kvname", "testkv" };

            bool flag = App.ProcessArgs(args, out string kvUrl, out string authType, out bool helpFlag);

            Assert.True(flag);
            Assert.False(helpFlag);
            Assert.Equal("https://testkv.vault.azure.net/", kvUrl);
            Assert.Equal("MSI", authType);
        }

        [Fact]
        public void KeyVaultNameAuthTypeTest()
        {
            string[] args = new string[] { "--kvname", "testkv", "--authtype", "CLI" };

            bool flag = App.ProcessArgs(args, out string kvUrl, out string authType, out bool helpFlag);

            Assert.True(flag);
            Assert.False(helpFlag);
            Assert.Equal("https://testkv.vault.azure.net/", kvUrl);
            Assert.Equal("CLI", authType);
        }

        [Fact]
        public void EnvVarValuesTest()
        {
            string[] args = Array.Empty<string>();
            Environment.SetEnvironmentVariable("KeyvaultName", "testkv");

            bool flag = App.ProcessArgs(args, out string kvUrl, out string authType, out bool helpFlag);
            Assert.True(flag);
            Assert.False(helpFlag);
            Assert.Equal("https://testkv.vault.azure.net/", kvUrl);
            Assert.Equal("MSI", authType);

            Environment.SetEnvironmentVariable("AUTH_TYPE", "CLI");
            flag = App.ProcessArgs(args, out kvUrl, out authType, out helpFlag);

            Assert.True(flag);
            Assert.False(helpFlag);
            Assert.Equal("https://testkv.vault.azure.net/", kvUrl);
            Assert.Equal("CLI", authType);

            Environment.SetEnvironmentVariable("KeyvaultName", null);
            Environment.SetEnvironmentVariable("AUTH_TYPE", null);

        }

        [Fact]
        public void InvalidCommandLineTest()
        {
            string[] args = new string[] { "--foo" };
            bool flag = App.ProcessArgs(args, out string kvUrl, out string authType, out bool helpFlag);
            Assert.False(flag);
            Assert.False(helpFlag);
            Assert.Null(kvUrl);
            Assert.Null(authType);

            args = new string[] { "--kvname" };
            flag = App.ProcessArgs(args, out kvUrl, out authType, out helpFlag);
            Assert.False(flag);
            Assert.False(helpFlag);
            Assert.Null(kvUrl);
            Assert.Null(authType);

            args = new string[] { "--kvname", "testkv", "--authtype" };
            flag = App.ProcessArgs(args, out kvUrl, out authType, out helpFlag);
            Assert.False(flag);
            Assert.False(helpFlag);
            Assert.Null(kvUrl);
            Assert.Null(authType);

            args = new string[] { "--kvname", "testkv", "--authtype", "CLI", "foo" };
            flag = App.ProcessArgs(args, out kvUrl, out authType, out helpFlag);
            Assert.False(flag);
            Assert.False(helpFlag);
            Assert.Null(kvUrl);
            Assert.Equal("CLI", authType);

            args = new string[] { "--kvname", "testkv", "--authtype", "CLI", "--foo" };
            flag = App.ProcessArgs(args, out kvUrl, out authType, out helpFlag);
            Assert.False(flag);
            Assert.False(helpFlag);
            Assert.Null(kvUrl);
            Assert.Equal("CLI", authType);
        }

    }
}
