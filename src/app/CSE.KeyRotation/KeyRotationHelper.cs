using CSE.Helium;
using CSE.Helium.DataAccessLayer;
using CSE.KeyVault;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Net;
using System.Threading;

namespace CSE.KeyRotation
{
    public class KeyRotationHelper : IKeyRotation
    {
        private readonly IDAL dal;
        private readonly IKeyVaultConnection keyVaultConnection;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        public AsyncRetryPolicy RetryCosmosPolicy { get; private set; }

        public KeyRotationHelper(IDAL dal, IKeyVaultConnection keyVaultConnection, IConfiguration configuration, ILogger<KeyRotationHelper> logger)
        {
            this.dal = dal;
            this.keyVaultConnection = keyVaultConnection;
            this.configuration = configuration;
            this.logger = logger;
            RetryCosmosPolicy = GetCosmosRetryPolicy();
        }

        /// <summary>
        /// This method creates a Polly retry policy when there is cosmos exception with code Unauthorized.
        /// </summary>
        /// <returns></returns>
        private AsyncRetryPolicy GetCosmosRetryPolicy()
        {
            return Policy.Handle<CosmosException>(e => e.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(Constants.RetryCount, async(exception, retryCount)  =>
                {

                    try
                    {
                        
                        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                        logger.LogInformation("Read the cosmos key from KeyVault.");
                        
                        //Get the latest cosmos key.                    
                        var cosmosKeySecret = await keyVaultConnection.Client.GetSecretAsync(keyVaultConnection.Address, Constants.CosmosKey).ConfigureAwait(false);

                        logger.LogInformation("Refresh cosmos connection with upadated secret.");
                        await dal.Reconnect(new Uri(configuration[Constants.CosmosUrl]), cosmosKeySecret.Value, configuration[Constants.CosmosDatabase], configuration[Constants.CosmosCollection]).ConfigureAwait(false);
                    }
                    finally
                    {
                        // release the semaphore
                        semaphoreSlim.Release();
                    }
                });
        }
    }

}
