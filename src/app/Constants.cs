namespace Helium
{
    /// <summary>
    /// String constants
    /// </summary>
    public class Constants
    {
        public static readonly string SwaggerVersion = "v " + Version.AssemblyVersion;
        public static readonly string SwaggerName = "helium";
        public static readonly string SwaggerTitle = "Helium (C#)";
        public static readonly string SwaggerPath = "/swagger/" + SwaggerName + "/swagger.json";
        public static readonly string XmlCommentsPath = SwaggerName + ".xml";

        public static readonly string KeyVaultName = "KeyVaultName";
        public static readonly string KeyVaultUrlError = "KeyVaultUrl not set";
        public static readonly string KeyVaultError = "Unable to read from Key Vault {0}";

        public static readonly string CosmosUrl = "CosmosUrl";
        public static readonly string CosmosUrlError = "CosmosUrl not set correctly {0}";

        public static readonly string CosmosKey = "CosmosKey";
        public static readonly string CosmosKeyError = "CosmosKey not set correctly {0}";

        public static readonly string CosmosDatabase = "CosmosDatabase";
        public static readonly string CosmosDatabaseError = "CosmosDatabase not set correctly {0}";

        public static readonly string CosmosCollection = "CosmosCollection";
        public static readonly string CosmosCollectionError = "CosmosCollection not set correctly {0}";

        public static readonly string AppInsightsKey = "AppInsightsKey";
        public static readonly string AppInsightsKeyError = "App Insights Key specified";

        // if port is changed, also update value in the Dockerfiles
        public static readonly string Port = "4120";

        public static readonly string HealthzResult = "Movies: 100\r\nActors: 553\r\nGenres: 20";
        public static readonly string HealthzError = "Healthz Failed:\r\n{0}";

        public static readonly int NotFound = 404;
        public static readonly int ServerError = 500;

        public static readonly int RotateKeyCheckSeconds = 600;
        public static int MainLoopSleepMs = 1000;
    }
}
