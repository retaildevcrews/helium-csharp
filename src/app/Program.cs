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
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Helium
{
    public sealed class App
    {
        // ILogger instance
        private static ILogger<App> _logger;

        // web host
        private static IWebHost _host;

        // Key Vault configuration
        private static IConfigurationRoot config = null;

        private static CancellationTokenSource ctCancel;

        public static void Stop()
        {
            if (ctCancel != null)
            {
                ctCancel.Cancel(false);
            }
        }


        /// <summary>
        /// Main entry point
        /// 
        /// Configure and run the web server
        /// </summary>
        /// <param name="args">command line args</param>
        public static async Task<int> Main(string[] args)
        {
            if (args == null) args = Array.Empty<string>();

            string cmd = string.Join(' ', args);

            string kv = Environment.GetEnvironmentVariable(Constants.KeyVaultName);
            string auth = Environment.GetEnvironmentVariable(Constants.AuthType);

            if (!string.IsNullOrEmpty(kv) && !cmd.Contains("--keyvault-name", StringComparison.Ordinal) && !cmd.Contains("-k", StringComparison.Ordinal))
            {
                cmd += " --keyvault-name " + kv;
            }

            if (!string.IsNullOrEmpty(auth) && !cmd.Contains("--auth-type", StringComparison.Ordinal) && !cmd.Contains("-a", StringComparison.Ordinal))
            {
                cmd += " --auth-type " + auth;
            }

            RootCommand root = new RootCommand
            {
                Name = "helium",
                Description = "A web app",
                TreatUnmatchedTokensAsErrors = true
            };

            root.AddOption(new Option(new string[] { "-k", "--keyvault-name" }, "The name or URL of the Azure Keyvault") { Argument = new Argument<string>(), Required = true });
            root.AddOption(new Option(new string[] { "-a", "--auth-type" }, "Authentication type - MSI or CLI") { Argument = new Argument<string>(() => "MSI") });
            root.AddOption(new Option(new string[] { "-d", "--dry-run" }, "Validate configuration but does not run web server"));

            root.Handler = CommandHandler.Create<string, string, bool>(RunApp);

            return await root.InvokeAsync(cmd).ConfigureAwait(false);
        }

        public static async Task<int> RunApp(string keyvaultName, string authType, bool dryRun)
        {
            // validate keyvaultName and convert to URL
            if (!KeyVaultHelper.BuildKeyVaultConnectionString(keyvaultName, out string kvUrl))
            {
                return -1;
            }

            // validate auth type
            if (!KeyVaultHelper.ValidateAuthType(authType))
            {
                Console.WriteLine($"Invalid AuthType specified: {authType}");
                return -1;
            }

            try
            {
                // setup ctl c handler
                ctCancel = SetupCtlCHandler();

                // build the host
                _host = await BuildHost(kvUrl, authType).ConfigureAwait(false);

                if (_host == null)
                {
                    return -1;
                }

                // don't start the web server
                if (dryRun)
                {
                    Console.WriteLine($"Version            {Middleware.VersionExtensions.Version}");
                    Console.WriteLine($"Keyvault           {kvUrl}");
                    Console.WriteLine($"Auth Type          {authType}");
                    Console.WriteLine($"Cosmos Server      {config.GetValue<string>(Constants.CosmosUrl)}");
                    Console.WriteLine($"Cosmos Key         Length({config.GetValue<string>(Constants.CosmosUrl).Length})");
                    Console.WriteLine($"Cosmos Database    {config.GetValue<string>(Constants.CosmosDatabase)}");
                    Console.WriteLine($"Cosmos Collection  {config.GetValue<string>(Constants.CosmosCollection)}");
                    Console.WriteLine($"App Insights Key   {(string.IsNullOrEmpty(config.GetValue<string>(Constants.AppInsightsKey)) ? "(not set" : "Length(" + config.GetValue<string>(Constants.AppInsightsKey).Length.ToString(CultureInfo.InvariantCulture))})");

                    return 0;
                }

                // log startup messages
                LogStartup();

                // start the webserver
                var w = _host.RunAsync();

                // this doesn't return except on ctl-c
                await RunKeyRotationCheck(ctCancel).ConfigureAwait(false);

                // if not cancelled, app exit -1
                return ctCancel.IsCancellationRequested ? 0 : -1;
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

                return -1;
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

            Console.WriteLine($"Version: {Middleware.VersionExtensions.Version}");
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
        /// <param name="authType">MSI, CLI or VS</param>
        /// <returns></returns>
        static async Task<KeyVaultClient> GetKeyVaultClient(string kvUrl, string authType)
        {
            // retry Managed Identity for 90 seconds
            //   AKS has to spin up an MI pod which can take a while the first time on the pod
            DateTime timeout = DateTime.Now.AddSeconds(90.0);

            // use MSI as default
            string authString;

            switch (authType)
            {
                case "MSI":
                    authString = "RunAs=App";
                    break;
                case "CLI":
                    authString = "RunAs=Developer; DeveloperTool=AzureCli";
                    break;
                case "VS":
                    authString = "RunAs=Developer; DeveloperTool=VisualStudio";
                    break;
                default:
                    Console.WriteLine("Invalid Key Vault Authentication Type");
                    return null;
            }

            while (true)
            {
                try
                {
                    var tokenProvider = new AzureServiceTokenProvider(authString);

                    // use Managed Identity (MSI) for secure access to Key Vault
                    var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback));

                    // read a key to make sure the connection is valid 
                    await keyVaultClient.GetSecretAsync(kvUrl, Constants.CosmosUrl).ConfigureAwait(false);

                    // return the client
                    return keyVaultClient;
                }
                catch (Exception ex)
                {
                    if (DateTime.Now <= timeout && authType == "MSI")
                    {
                        // retry MSI connections for pod identity

                        Console.WriteLine($"KeyVault:Retry");
                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                    else
                    {
                        // log and fail

                        Console.WriteLine($"KeyVault:Exception: {ex.Message}\n{ex}");
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Build the web host
        /// </summary>
        /// <param name="kvUrl">URL of the Key Vault</param>
        /// <param name="authType">MSI, CLI, VS</param>
        /// <returns>Web Host ready to run</returns>
        static async Task<IWebHost> BuildHost(string kvUrl, string authType)
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
    }
}
