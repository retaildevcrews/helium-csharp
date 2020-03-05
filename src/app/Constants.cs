namespace Helium
{
    /// <summary>
    /// String constants
    /// </summary>
    public sealed class Constants
    {
        public static readonly string SwaggerVersion = "v " + Version.AssemblyVersion;
        public const string SwaggerName = "helium";
        public const string SwaggerTitle = "Helium (C#)";
        public const string SwaggerPath = "/swagger/" + SwaggerName + "/swagger.json";
        public const string XmlCommentsPath = SwaggerName + ".xml";

        public const string KeyVaultName = "KeyVaultName";
        public const string AuthType = "AUTH_TYPE";

        public const string CosmosCollection = "CosmosCollection";
        public const string CosmosDatabase = "CosmosDatabase";
        public const string CosmosKey = "CosmosKey";
        public const string CosmosUrl = "CosmosUrl";

        public const string AppInsightsKey = "AppInsightsKey";
        public const string NewKeyLoadedMetric = "newKeyLoaded";

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
    }
}
