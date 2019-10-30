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

        public static readonly string CosmosUrl = "CosmosUrl";
        public static readonly string CosmosKey = "CosmosKey";
        public static readonly string CosmosDatabase = "CosmosDatabase";
        public static readonly string CosmosCollection = "CosmosCollection";

        public static readonly string AppInsightsKey = "AppInsightsKey";

        public static readonly string DALReloadError = "DAL reload failed";

        // if port is changed, also update value in the Dockerfiles
        public static readonly string Port = "4120";

        public static readonly int RotateKeyCheckSeconds = 30;
        public static int MainLoopSleepMs = 1000;

        public static readonly string ActorsControllerException = "ActorsControllerException";
        public static readonly string GenresControllerException = "GenresControllerException";
        public static readonly string HealthzControllerException = "HealthzControllerException";
        public static readonly string MoviesControllerException = "MoviesControllerException";
        public static readonly string FeaturedControllerException = "FeaturedControllerException";
    }
}
