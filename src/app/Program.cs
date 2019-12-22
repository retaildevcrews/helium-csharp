using Helium.DataAccessLayer;
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
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Helium
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303")]
    public sealed class App
    {
        // ILogger instance
        private static ILogger<App> _logger;

        // web host
        private static IWebHost _host;

        // Key Vault configuration
        private static IConfigurationRoot config = null;


        /// <summary>
        /// Main entry point
        /// 
        /// Configure and run the web server
        /// </summary>
        /// <param name="args">command line args</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1303")]
        public static async Task Main(string[] args)
        {
            try
            {
                // get the Key Vault URL
                string kvUrl = GetKeyVaultUrl(args);

                // kvUrl is required
                if (string.IsNullOrEmpty(kvUrl))
                {
                    Console.WriteLine("Key Vault name missing");

                    Environment.Exit(-1);
                }

                // setup ctl c handler
                using CancellationTokenSource ctCancel = SetupCtlCHandler();

                // build the host
                _host = await BuildHost(kvUrl).ConfigureAwait(false);

                // log startup messages
                LogStartup();

                // start the webserver
                var w = _host.RunAsync();

                // this doesn't return except on ctl-c
                await RunKeyRotationCheck(ctCancel).ConfigureAwait(false);
            }

            catch (Exception ex)
            {
                // end app on error
                if (_logger != null)
                {
                    _logger.LogError($"Exception: {ex}");
                }
                else
                {
                    Console.WriteLine($"Error in Main()\n{ex}");
                }

                Environment.Exit(-1);
            }
        }


        /// <summary>
        /// Check for Cosmos key rotation
        /// </summary>
        /// <param name="ctCancel">CancellationTokenSource</param>
        /// <returns>Only returns when ctl-c is pressed and cancellation token is cancelled</returns>
        static async Task RunKeyRotationCheck(CancellationTokenSource ctCancel)
        {
            string key = config[Constants.CosmosKey];

            // reload Key Vault values
            while (!ctCancel.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(Constants.KeyVaultChangeCheckSeconds * 1000, ctCancel.Token).ConfigureAwait(false);

                    if (!ctCancel.IsCancellationRequested)
                    {
                        // reload the config from Key Vault
                        config.Reload();

                        // if the key changed
                        if (!ctCancel.IsCancellationRequested)
                        {
                            // reconnect the DAL
                            var dal = _host.Services.GetService<IDAL>();

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
                                        var telemetryClient = _host.Services.GetService<TelemetryClient>();

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

                // give threads a chance to shutdown
                Thread.Sleep(500);

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
            _logger = _host.Services.GetRequiredService<ILogger<App>>();

            if (_logger != null)
            {
                // get the IConfigurationRoot from DI
                var cfg = _host.Services.GetService<IConfigurationRoot>();

                // log a not using app insights warning
                if (string.IsNullOrEmpty(cfg.GetValue<string>(Constants.AppInsightsKey)))
                {
                    _logger.LogWarning("App Insights Key not set");
                }

                _logger.LogInformation("Web Server Started");
            }

            Console.WriteLine($"Version: {Version.AssemblyVersion}");
        }

        /// <summary>
        /// Builds the config for the web server
        /// 
        /// Uses Key Vault via Managed Identity (MSI)
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

                Console.WriteLine($"KeyVault:Exception: {ex.Message}\n{ex}");
                Environment.Exit(-1);
            }

            return null;
        }

        /// <summary>
        /// Get a valid key vault client
        /// AKS takes time to spin up the first pod identity, so
        ///   we retry for up to 90 seconds
        /// </summary>
        /// <param name="kvUrl">URL of the key vault</param>
        /// <returns></returns>
        static async Task<KeyVaultClient> GetKeyVaultClient(string kvUrl)
        {
            // retry Managed Identity for 90 seconds
            //   AKS has to spin up an MI pod which can take a while the first time on the pod
            DateTime timeout = DateTime.Now.AddSeconds(90.0);

            while (true)
            {
                try
                {
                    // use Managed Identity (MSI) for secure access to Key Vault
                    var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));

                    // read a key to make sure the connection is valid 
                    await keyVaultClient.GetSecretAsync(kvUrl, Constants.CosmosUrl).ConfigureAwait(false);

                    // return the client
                    return keyVaultClient;
                }
                catch (Exception ex)
                {
                    if (DateTime.Now <= timeout)
                    {
                        // log and retry

                        Console.WriteLine($"KeyVault:Retry: {ex.Message}");
                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                    else
                    {
                        // log and fail

                        Console.WriteLine($"KeyVault:Exception: {ex.Message}\n{ex}");
                        Environment.Exit(-1);
                    }
                }
            }
        }

        /// <summary>
        /// Build the web host
        /// </summary>
        /// <param name="kvUrl">URL of the Key Vault</param>
        /// <returns>Web Host ready to run</returns>
        static async Task<IWebHost> BuildHost(string kvUrl)
        {
            // create the Key Vault Client
            using var kvClient = await GetKeyVaultClient(kvUrl).ConfigureAwait(false);

            // build the config
            // we need the key vault values for the DAL
            config = BuildConfig(kvClient, kvUrl);

            // configure the web host builder
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .UseUrls(string.Format($"http://*:{Constants.Port}/"))
                .UseStartup<Startup>()
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
                });

            // build the host
            return builder.Build();
        }

        /// <summary>
        /// Get the Key Vault URL from the environment variable or command line
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>URL to Key Vault</returns>
        static string GetKeyVaultUrl(string[] args)
        {
            // get the key vault name from the environment variable
            string kvName = Environment.GetEnvironmentVariable(Constants.KeyVaultName);

            // command line arg overrides environment variable
            if (args.Length > 0 && !args[0].StartsWith("-", StringComparison.OrdinalIgnoreCase))
            {
                kvName = args[0].Trim();
            }

            if (kvName == null || string.IsNullOrEmpty(kvName.Trim()))
            {
                return string.Empty;
            }

            return KeyVaultHelper.BuildKeyVaultConnectionString(kvName);
        }
    }
}
