// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace CSE.Helium
{
    /// <summary>
    /// Application constants
    /// </summary>
    public sealed class Constants
    {
        public const string SwaggerTitle = "Helium";
        public const string SwaggerPath = "/swagger/helium.json";

        public const string KeyVaultName = "KEYVAULT_NAME";
        public const string AuthType = "AUTH_TYPE";
        public const string LogLevel = "LOG_LEVEL";

        public const string CosmosCollection = "CosmosCollection";
        public const string CosmosDatabase = "CosmosDatabase";
        public const string CosmosKey = "CosmosKey";
        public const string CosmosUrl = "CosmosUrl";

        public const string AppInsightsKey = "AppInsightsKey";
        public const string NewKeyLoadedMetric = "newKeyLoaded";

        public const string CosmosTimeout = "CosmosOptions:Timeout";
        public const string CosmosRetry = "CosmosOptions:Retries";

        // if port is changed, also update value in the Dockerfiles
        public const string Port = "4120";

        public const int KeyVaultChangeCheckSeconds = 30;

        public const string ActorsControllerException = "ActorsControllerException";
        public const string GenresControllerException = "GenresControllerException";
        public const string HealthzControllerException = "HealthzControllerException";
        public const string MoviesControllerException = "MoviesControllerException";
        public const string FeaturedControllerException = "FeaturedControllerException";

        public const int DefaultPageSize = 100;
        public const int MaxPageSize = 1000;

        public const int HealthzCacheDuration = 60;
        public const int GracefulShutdownTimeout = 10;

        // If cosmos key is rotatated, number of retries with new key.
        public const int RetryCount = 1;
    }
}
