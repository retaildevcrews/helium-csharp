// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CSE.Helium.DataAccessLayer;
using CSE.KeyRotation;
using CSE.KeyVault;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CSE.Helium
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        // ILogger instance
        private static ILogger<App> logger;

        // web host
        private static IWebHost host;

        // Key Vault configuration
        private static IConfigurationRoot config;

        private static CancellationTokenSource ctCancel;

        /// <summary>
        /// Gets or sets LogLevel
        /// </summary>
        public static LogLevel AppLogLevel { get; set; } = LogLevel.Warning;

        /// <summary>
        /// Gets or sets a value indicating whether LogLevel is set in command line or env var
        /// </summary>
        public static bool IsLogLevelSet { get; set; }

        /// <summary>
        /// Main entry point
        ///
        /// Configure and run the web server
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>IActionResult</returns>
        public static async Task<int> Main(string[] args)
        {
            // build the System.CommandLine.RootCommand
            RootCommand root = BuildRootCommand();
            root.Handler = CommandHandler.Create<string, AuthenticationType, LogLevel, bool>(RunApp);

            // run the app
            return await root.InvokeAsync(CombineEnvVarsWithCommandLine(args)).ConfigureAwait(false);
        }

        /// <summary>
        /// Stop the web server via code
        /// </summary>
        public static void Stop()
        {
            if (ctCancel != null)
            {
                ctCancel.Cancel(false);
            }
        }

        /// <summary>
        /// Creates a CancellationTokenSource that cancels on ctl-c pressed
        /// </summary>
        /// <returns>CancellationTokenSource</returns>
        private static CancellationTokenSource SetupCtlCHandler()
        {
            CancellationTokenSource ctCancel = new CancellationTokenSource();

            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true;
                ctCancel.Cancel();

                Console.WriteLine("Ctl-C Pressed - Starting shutdown ...");

                // trigger graceful shutdown for the webhost
                // force shutdown after timeout, defined in UseShutdownTimeout within BuildHost() method
                await host.StopAsync().ConfigureAwait(false);

                // end the app
                Environment.Exit(0);
            };

            return ctCancel;
        }

        /// <summary>
        /// Log startup messages
        /// </summary>
        private static void LogStartup()
        {
            // get the logger service
            logger = host.Services.GetRequiredService<ILogger<App>>();

            if (logger != null)
            {
                // get the IConfigurationRoot from DI
                IConfigurationRoot cfg = host.Services.GetService<IConfigurationRoot>();

                // log a not using app insights warning
                if (string.IsNullOrEmpty(cfg.GetValue<string>(Constants.AppInsightsKey)))
                {
                    logger.LogWarning("App Insights Key not set");
                }

                logger.LogInformation("Web Server Started");
            }

            Console.WriteLine("\n");
            Console.WriteLine("                                ,-\"\"\"\"-.");
            Console.WriteLine("                              ,'      _ `.");
            Console.WriteLine("                             /       )_)  \\");
            Console.WriteLine("                            :              :");
            Console.WriteLine("                            \\              /");
            Console.WriteLine(" _          _ _              \\            /");
            Console.WriteLine("| |        | (_)              `.        ,'");
            Console.WriteLine("| |__   ___| |_ _   _ _ __ ___  `.    ,'");
            Console.WriteLine("| '_ \\ / _ \\ | | | | | '_ ` _ \\   `.,'");
            Console.WriteLine("| | | |  __/ | | |_| | | | | | |   /\\`.   ,-._");
            Console.WriteLine("|_| |_|\\___|_|_|\\__,_|_| |_| |_|        `-'");

            Console.WriteLine($"\nVersion: {Middleware.VersionExtensions.Version}");
        }

        /// <summary>
        /// Builds the config for the web server
        ///
        /// Uses Key Vault via Managed Identity (MI)
        /// </summary>
        /// <param name="kvClient">Key Vault Client</param>
        /// <param name="kvUrl">Key Vault URL</param>
        /// <returns>Root Configuration</returns>
        private static IConfigurationRoot BuildConfig(KeyVaultClient kvClient, string kvUrl)
        {
            try
            {
                // standard config builder
                IConfigurationBuilder cfgBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)

                    // use Azure Key Vault
                    .AddAzureKeyVault(kvUrl, kvClient, new DefaultKeyVaultSecretManager());

                // build the config
                return cfgBuilder.Build();
            }
            catch (Exception ex)
            {
                // log and fail
                Console.WriteLine($"{ex}\nBuildConfig:Exception: {ex.Message}");
                Environment.Exit(-1);
            }

            return null;
        }

        /// <summary>
        /// Build the web host
        /// </summary>
        /// <param name="kvUrl">URL of the Key Vault</param>
        /// <param name="authType">MI, CLI, VS</param>
        /// <returns>Web Host ready to run</returns>
        private static async Task<IWebHost> BuildHost(string kvUrl, AuthenticationType authType)
        {
            // create the Key Vault Client
            KeyVaultClient kvClient = await KeyVaultHelper.GetKeyVaultClient(kvUrl, authType, Constants.CosmosDatabase).ConfigureAwait(false);

            if (kvClient == null)
            {
                return null;
            }

            // build the config
            // we need the key vault values for the DAL
            config = BuildConfig(kvClient, kvUrl);

            // configure the web host builder
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder()
                .UseConfiguration(config)
                .UseUrls(string.Format(System.Globalization.CultureInfo.InvariantCulture, $"http://*:{Constants.Port}/"))
                .UseStartup<Startup>()
                .UseShutdownTimeout(TimeSpan.FromSeconds(Constants.GracefulShutdownTimeout))
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IServiceCollection>(services);

                    services.AddKeyRotation();

                    // add the data access layer via DI
                    services.AddDal(
                        new Uri(config.GetValue<string>(Constants.CosmosUrl)),
                        config.GetValue<string>(Constants.CosmosKey),
                        config.GetValue<string>(Constants.CosmosDatabase),
                        config.GetValue<string>(Constants.CosmosCollection),
                        config.GetValue<int>(Constants.CosmosTimeout),
                        config.GetValue<int>(Constants.CosmosRetry));

                    // add the KeyVaultConnection via DI
                    services.AddKeyVaultConnection(kvClient, kvUrl);

                    // add IConfigurationRoot
                    services.AddSingleton<IConfigurationRoot>(config);
                    services.AddResponseCaching();
                });

            // configure logger based on command line
            builder.ConfigureLogging(logger =>
            {
                logger.ClearProviders();
                logger.AddConsole();

                // if you specify the --log-level option, it will override the appsettings.json options
                // remove any or all of the code below that you don't want to override
                if (App.IsLogLevelSet)
                {
                    logger.AddFilter("Microsoft", AppLogLevel)
                    .AddFilter("System", AppLogLevel)
                    .AddFilter("Default", AppLogLevel)
                    .AddFilter("CSE.Helium", AppLogLevel);
                }
            });

            // build the host
            return builder.Build();
        }
    }
}
