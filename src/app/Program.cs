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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Helium
{
    public class App
    {
        // ILogger instance
        private static ILogger<App> _logger;

        // web host
        private static IWebHost _host;

        // secret manager
        private static readonly EventingKeyVaultSecretManager _secretManager = new EventingKeyVaultSecretManager();

        /// <summary>
        /// Main entry point
        /// 
        /// Configure and run the web server
        /// </summary>
        /// <param name="args">command line args</param>
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

                // add the key change handler
                _secretManager.KeyVaultSecretChanged += KeyVaultSecretChangedHandler;

                // build the host
                _host = await BuildHost(kvUrl);

                // log startup messages
                LogStartup();

                // run the web server
                _host.Run();
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
        /// Gets fired when a key vault secret changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Key Name that changed</param>
        static void KeyVaultSecretChangedHandler(Object sender, KeyVaultSecretChangedEventArgs e)
        {
            // list of cosmos key vault secrets to watch for changes
            List<string> cosmosKeyNames = new List<string> { Constants.CosmosCollection, Constants.CosmosDatabase, Constants.CosmosKey, Constants.CosmosUrl };

            // get the IConfigurationRoot from DI
            var cfg = _host.Services.GetRequiredService<IConfiguration>() as IConfigurationRoot;

            // reload config
            cfg.Reload();

            Console.Write($"{DateTime.Now.ToString("hh:mm:ss")}  ");

            foreach (string key in e.KeysChanged)
            {
                Console.Write($"{key} ");
            }
            Console.WriteLine();

            foreach (string key in e.KeysChanged)
            {
                // reload the DAL if the Cosmos info changed
                if (cosmosKeyNames.Contains(key))
                {
                    try
                    {
                        // reconnect to Cosmos with the new key
                        _host.Services.GetService<IDAL>().Reconnect(cfg.GetValue<string>(Constants.CosmosUrl),
                            cfg.GetValue<string>(Constants.CosmosKey),
                            cfg.GetValue<string>(Constants.CosmosDatabase),
                            cfg.GetValue<string>(Constants.CosmosCollection));

                        _logger.LogInformation("DAL.Reconnect");

                        // Send a NewKeyLoadedMetric to App Insights
                        if (!string.IsNullOrEmpty(cfg.GetValue<string>(Constants.AppInsightsKey)))
                        {
                            var telemetryClient = _host.Services.GetService<TelemetryClient>();

                            if (telemetryClient != null)
                            {
                                telemetryClient.TrackMetric(Constants.NewKeyLoadedMetric, 1);
                            }
                        }

                        // only show the first 5 characters of the Cosmos key
                        string val = key == Constants.CosmosKey ? cfg.GetValue<string>(key).Substring(0, 5) + "..." : cfg.GetValue<string>(key);

                        Console.WriteLine($"DAL.Reconnect: {key} = {val}");

                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"DAL.Reconnect Exception: Original values still in use\n{ex}");
                    }
                }
            }
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
                var cfg = _host.Services.GetRequiredService<IConfiguration>() as IConfigurationRoot;

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
        /// <param name="kvOptions">AzureKeyVaultConfigurationOptions</param>
        /// <returns>Root Configuration</returns>
        static IConfigurationRoot BuildConfig(AzureKeyVaultConfigurationOptions kvOptions)
        {
            // standard config builder
            var cfgBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            try
            {
                // use Azure Key Vault
                cfgBuilder.AddAzureKeyVault(kvOptions);

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
                    await keyVaultClient.GetSecretAsync(kvUrl, Constants.CosmosUrl);

                    // return the client
                    return keyVaultClient;
                }
                catch (Exception ex)
                {
                    if (DateTime.Now <= timeout)
                    {
                        // log and retry

                        Console.WriteLine($"KeyVault:Retry: {ex.Message}");
                        await Task.Delay(1000);
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
            AzureKeyVaultConfigurationOptions kvOptions = new AzureKeyVaultConfigurationOptions
            {
                Client = await GetKeyVaultClient(kvUrl),
                Manager = _secretManager,
                ReloadInterval = TimeSpan.FromSeconds(Constants.KeyVaultChangeCheckSeconds),
                Vault = kvUrl
            };

            // build the config
            // we need the key vault values for the DAL
            var config = BuildConfig(kvOptions);

            // configure the web host builder
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .UseUrls(string.Format($"http://*:{Constants.Port}/"))
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    // add the data access layer via DI
                    services.AddDal(config.GetValue<string>(Constants.CosmosUrl),
                        config.GetValue<string>(Constants.CosmosKey),
                        config.GetValue<string>(Constants.CosmosDatabase),
                        config.GetValue<string>(Constants.CosmosCollection));

                    // add the KeyVaultConnection via DI
                    services.AddKeyVaultConnection(kvOptions.Client, kvUrl);
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
            if (args.Length > 0 && !args[0].StartsWith("-"))
            {
                kvName = args[0].Trim();
            }

            if (kvName == null || string.IsNullOrEmpty(kvName.Trim()))
            {
                return string.Empty;
            }

            return KeyVaultHelper.BuildKeyVaultUrl(kvName);
        }
    }
}
