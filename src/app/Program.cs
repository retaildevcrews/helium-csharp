using CSE.Helium.DataAccessLayer;
using KeyVault.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CSE.Helium
{
    public sealed partial class App
    {
        // ILogger instance
        private static ILogger<App> logger;

        // web host
        private static IWebHost host;

        // Key Vault configuration
        private static IConfigurationRoot config = null;

        private static CancellationTokenSource ctCancel;

        public static LogLevel HeliumLogLevel { get; set; } = LogLevel.Warning;

        /// <summary>
        /// Main entry point
        /// 
        /// Configure and run the web server
        /// </summary>
        /// <param name="args">command line args</param>
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

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                ctCancel.Cancel();

                Console.WriteLine("Ctl-C Pressed - Starting shutdown ...");

                // trigger graceful shutdown for the webhost
                // force shutdown after timeout, defined in UseShutdownTimeout within BuildHost() method
                host.StopAsync();
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
                var cfg = host.Services.GetService<IConfigurationRoot>();

                // log a not using app insights warning
                if (string.IsNullOrEmpty(cfg.GetValue<string>(Constants.AppInsightsKey)))
                {
                    logger.LogWarning("App Insights Key not set");
                }

                logger.LogInformation("Web Server Started");
            }

            Console.WriteLine($"Version: {Middleware.VersionExtensions.Version}");
        }

        /// <summary>
        /// Builds the config for the web server
        /// 
        /// Uses Key Vault via Managed Identity (MI)
        /// </summary>
        /// <param name="kvClient">Key Vault Client</param>
        /// <param name="kvUrl">Key Vault URL</param>
        /// <returns>Root Configuration</returns>
        static IConfigurationRoot BuildConfig(KeyVaultClient kvClient, string kvUrl)
        {
            try
            {
                // standard config builder
                var cfgBuilder = new ConfigurationBuilder()
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
        static async Task<IWebHost> BuildHost(string kvUrl, AuthenticationType authType)
        {
            // create the Key Vault Client
            var kvClient = await GetKeyVaultClient(kvUrl, authType).ConfigureAwait(false);

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
                .UseKestrel()
                .UseUrls(string.Format(System.Globalization.CultureInfo.InvariantCulture, $"http://*:{Constants.Port}/"))
                .UseStartup<Startup>()
                .UseShutdownTimeout(TimeSpan.FromSeconds(10))
                .ConfigureServices(services =>
                {
                    // add the data access layer via DI
                    services.AddDal(new Uri(config.GetValue<string>(Constants.CosmosUrl)),
                        config.GetValue<string>(Constants.CosmosKey),
                        config.GetValue<string>(Constants.CosmosDatabase),
                        config.GetValue<string>(Constants.CosmosCollection));

                    // add the KeyVaultConnection via DI
                    services.AddKeyVaultConnection(kvClient, new Uri(kvUrl));

                    // add IConfigurationRoot
                    services.AddSingleton<IConfigurationRoot>(config);

                    services.AddResponseCaching();
                })
                // configure logger based on command line
                .ConfigureLogging(logger =>
                {
                    logger.ClearProviders();
                    logger.AddConsole()
                    .AddFilter("Microsoft", HeliumLogLevel)
                    .AddFilter("System", HeliumLogLevel)
                    .AddFilter("Default", HeliumLogLevel);
                });

            // build the host
            return builder.Build();
        }

        /// <summary>
        /// Get a valid key vault client
        /// AKS takes time to spin up the first pod identity, so
        ///   we retry for up to 90 seconds
        /// </summary>
        /// <param name="kvUrl">URL of the key vault</param>
        /// <param name="authType">MI, CLI or VS</param>
        /// <returns></returns>
        static async Task<KeyVaultClient> GetKeyVaultClient(string kvUrl, AuthenticationType authType)
        {
            // retry Managed Identity for 90 seconds
            //   AKS has to spin up an MI pod which can take a while the first time on the pod
            DateTime timeout = DateTime.Now.AddSeconds(90.0);

            // use MI as default
            string authString = "RunAs=App";

#if (DEBUG)
            // Only support CLI and VS credentials in debug mode
            switch (authType)
            {
                case AuthenticationType.CLI:
                    authString = "RunAs=Developer; DeveloperTool=AzureCli";
                    break;
                case AuthenticationType.VS:
                    authString = "RunAs=Developer; DeveloperTool=VisualStudio";
                    break;
            }
#else
            if (authType != AuthenticationType.MI)
            {
                Console.WriteLine("Release builds require MI authentication for Key Vault");
                return null;
            }
#endif

            while (true)
            {
                try
                {
                    var tokenProvider = new AzureServiceTokenProvider(authString);

                    // use Managed Identity (MI) for secure access to Key Vault
                    var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback));

                    // read a key to make sure the connection is valid 
                    await keyVaultClient.GetSecretAsync(kvUrl, Constants.CosmosUrl).ConfigureAwait(false);

                    // return the client
                    return keyVaultClient;
                }
                catch (Exception ex)
                {
                    if (DateTime.Now <= timeout && authType == AuthenticationType.MI)
                    {
                        // retry MI connections for pod identity

#if (DEBUG)
                        // Don't retry in debug mode
                        Console.WriteLine($"KeyVault:Exception: Unable to connect to Key Vault using MI");
                        return null;
#else
                        Console.WriteLine($"KeyVault:Retry");
                        await Task.Delay(1000).ConfigureAwait(false);
#endif
                    }
                    else
                    {
                        // log and fail

                        Console.WriteLine($"{ex}\nKeyVault:Exception: {ex.Message}");
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Check for Cosmos key rotation
        /// </summary>
        /// <param name="ctCancel">CancellationTokenSource</param>
        /// <returns>Only returns when ctl-c is pressed and cancellation token is cancelled</returns>
        static async Task RunKeyRotationCheck(CancellationTokenSource ctCancel, int checkEverySeconds)
        {
            string key = config[Constants.CosmosKey];

            // reload Key Vault values
            while (!ctCancel.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(checkEverySeconds * 1000, ctCancel.Token).ConfigureAwait(false);

                    if (!ctCancel.IsCancellationRequested)
                    {
                        // reload the config from Key Vault
                        config.Reload();

                        // if the key changed
                        if (!ctCancel.IsCancellationRequested)
                        {
                            // reconnect the DAL
                            var dal = host.Services.GetService<IDAL>();

                            if (dal != null)
                            {
                                // this will only reconnect if the variables changed
                                await dal.Reconnect(new Uri(config[Constants.CosmosUrl]), config[Constants.CosmosKey], config[Constants.CosmosDatabase], config[Constants.CosmosCollection]).ConfigureAwait(false);

                                if (key != config[Constants.CosmosKey])
                                {
                                    key = config[Constants.CosmosKey];
                                    Console.WriteLine("Cosmos Key Rotated");

                                    // send a NewKeyLoadedMetric to App Insights
                                    if (!string.IsNullOrEmpty(config[Constants.AppInsightsKey]))
                                    {
                                        var telemetryClient = host.Services.GetService<TelemetryClient>();

                                        if (telemetryClient != null)
                                        {
                                            telemetryClient.TrackMetric(Constants.NewKeyLoadedMetric, 1);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // continue running with existing key
                    Console.WriteLine($"Cosmos Key Rotate Exception - using existing connection");
                }
            }
        }
    }
}
