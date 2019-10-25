using Helium.DataAccessLayer;
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
    public class App
    {
        // application configuration
        private static IConfigurationRoot config;

        // ILogger instance
        private static ILogger<App> logger;

        // web host
        private static IWebHost host;

        // data access layer
        private static DAL dal;

        // CosmosDB key
        private static string cosmosKey;

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

                // build the config first so Key Vault secrets are available to dependent services
                config = await BuildConfig(kvUrl);

                // Create data access layer
                dal = CreateDal();

                // validate the CosmosDB connection / collection
                dal.GetHealthz();

                // build the host
                host = BuildHost(kvUrl);

                // run the web app
                RunWebApp();
            }

            catch (Exception ex)
            {
                // end app on error
                if (logger != null)
                {
                    logger.LogError("Exception: {0}", ex);
                }
                else
                {
                    Console.WriteLine("Error in Main()\r\n{0}", ex);
                }

                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Loop until ctl c is pressed
        /// Check for Cosmos key rotation
        /// </summary>
        static void RunWebApp()
        {
            // setup ctl c handler
            bool cancel = false;
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                Console.WriteLine("Ctl-C Pressed - Starting shutdown ...");
                e.Cancel = true;
                cancel = true;
            };

            // log startup messages
            LogStartup();

            // run the web server in the background
            host.RunAsync();

            // get the current CosmosDB key for comparison
            cosmosKey = config.GetValue<string>(Constants.CosmosKey);

            // setup next key rotation check time
            DateTime nextCheck = DateTime.Now.AddSeconds(Constants.RotateKeyCheckSeconds);

            // make sure sleep isn't too long
            Constants.MainLoopSleepMs = Constants.MainLoopSleepMs > 5000 ? 5000 : Constants.MainLoopSleepMs;
            // run until cancelled
            while (!cancel)
            {
                // if time to check for key rotation
                if (nextCheck <= DateTime.Now)
                {
                    nextCheck = ReloadDal();
                }

                // sleep
                if (!cancel)
                {
                    System.Threading.Thread.Sleep(Constants.MainLoopSleepMs);
                }
            }
        }

        /// <summary>
        /// Log startup messages
        /// </summary>
        private static void LogStartup()
        {
            // get the logger service
            logger = host.Services.GetRequiredService<ILogger<App>>();

            // log a not using app insights warning
            if (string.IsNullOrEmpty(config.GetValue<string>(Constants.AppInsightsKey)))
            {
                logger.LogWarning("App Insights Key not set");
            }

            logger.LogInformation("Web Server Started");
            Console.WriteLine("Version: {0}", Version.AssemblyVersion);
        }

        /// <summary>
        /// Reload the data access layer if the Cosmos key changed
        /// </summary>
        /// <returns></returns>
        private static DateTime ReloadDal()
        {
            // reload config from Key Vault
            config.Reload();

            // only reload dal if key changed
            string newCosmosKey = config.GetValue<string>(Constants.CosmosKey);
            if (newCosmosKey != cosmosKey)
            {
                // create a new data access layer
                DAL d = CreateDal();

                // don't reset if it failed
                if (d != null)
                {
                    try
                    {
                        // run a test query
                        dal.GetHealthz();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(Constants.DALReloadError);
                        logger.LogError(ex.ToString());
                    }

                    dal = d;

                    logger.LogInformation("DAL reloaded");
                    Console.WriteLine("Cosmos key rotated from {0}... to {1}...", cosmosKey.Substring(0, 5), newCosmosKey.Substring(0, 5));
                }
                else
                {
                    logger.LogError(Constants.DALReloadError);
                }
            }

            // set next check time
            return DateTime.Now.AddSeconds(Constants.RotateKeyCheckSeconds);
        }

        /// <summary>
        /// Builds the config for the web server
        /// 
        /// Uses Key Vault via Managed Identity (MSI)
        /// </summary>
        /// <param name="kvUrl">URL of the Key Vault to use</param>
        /// <returns>Root Configuration</returns>
        static async Task<IConfigurationRoot> BuildConfig(string kvUrl)
        {
            // standard config builder
            var cfgBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            // retry Managed Identity for 90 seconds
            DateTime timeout = DateTime.Now.AddSeconds(90.0);

            while (true)
            {
                // AKS has to spin up an MI pod which can take a while

                try
                {
                    // use Managed Identity (MSI) for secure access to Key Vault
                    var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));

                    // read a key to make sure the connection is valid 
                    await keyVaultClient.GetSecretAsync(kvUrl, Constants.CosmosUrl);

                    // use Azure Key Vault
                    cfgBuilder.AddAzureKeyVault(kvUrl, keyVaultClient, new DefaultKeyVaultSecretManager());

                    // build the config
                    return cfgBuilder.Build();
                }
                catch (Exception ex)
                {
                    if (DateTime.Now <= timeout)
                    {
                        // log and retry

                        Console.WriteLine("KeyVault:Retry: {0}", ex.Message);
                        Thread.Sleep(500);
                    }
                    else
                    {
                        // log and fail

                        Console.WriteLine("KeyVault:Exception: {0}\r\n{1}", ex.Message, ex);
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
        static IWebHost BuildHost(string kvUrl)
        {
            // configure the web host builder
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseStartup<Startup>();

            // use the configuration built from Key Vault
            builder.UseConfiguration(config);

            // setup the listen port
            UseUrls(builder);

            // add the data access layer
            UseDal(builder);

            // build the host
            return builder.Build();
        }

        /// <summary>
        /// Set the listen URL and port
        /// </summary>
        /// <param name="builder">Web Host Builder</param>
        static void UseUrls(IWebHostBuilder builder)
        {
            // listen on the default port
            builder.UseUrls(string.Format("http://*:{0}/", Constants.Port));
        }

        /// <summary>
        /// Create the data access layer from the Key Vault config values
        /// </summary>
        /// <returns></returns>
        static DAL CreateDal()
        {
            // get the variables from Key Vault / dotnet secrets
            string cosmosUrl = config.GetValue<string>(Constants.CosmosUrl);
            string cosmosKey = config.GetValue<string>(Constants.CosmosKey);
            string cosmosDatabase = config.GetValue<string>(Constants.CosmosDatabase);
            string cosmosCollection = config.GetValue<string>(Constants.CosmosCollection);

            // validate required parameters
            if (string.IsNullOrEmpty(cosmosUrl))
            {
                throw new ArgumentException(string.Format("CosmosUrl not set correctly {0}", cosmosUrl));
            }

            if (string.IsNullOrEmpty(cosmosKey))
            {
                throw new ArgumentException(string.Format("CosmosKey not set correctly {0}", cosmosKey));
            }

            if (string.IsNullOrEmpty(cosmosDatabase))
            {
                throw new ArgumentException(string.Format("CosmosDatabase not set correctly {0}", cosmosDatabase));
            }

            if (string.IsNullOrEmpty(cosmosCollection))
            {
                throw new ArgumentException(string.Format("CosmosCollection not set correctly {0}", cosmosCollection));
            }

            return new DAL(cosmosUrl, cosmosKey, cosmosDatabase, cosmosCollection);
        }

        /// <summary>
        /// Add the Data Access Layer as a singleton for use in controllers
        /// </summary>
        /// <param name="builder">Web Host Builder</param>
        static void UseDal(IWebHostBuilder builder)
        {
            // add the data access layer as a singleton
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IDAL>(dal);
            });
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

            // build the URL
            if (!string.IsNullOrEmpty(kvName) && !kvName.StartsWith("https://"))
            {
                return "https://" + kvName + ".vault.azure.net/";
            }

            return string.Empty;
        }
    }
}
