namespace Helium.Model
{
    public class HealthzSuccess
    {
        public string status { get; set; } = "UP";
        public SuccessDetails details { get; set; } = new SuccessDetails();
    }

    public class HealthzError
    {
        public string status { get; set; } = "DOWN";
        public ErrorDetails details { get; set; } = new ErrorDetails();
    }

    public class SuccessDetails
    {
        public CosmosDbSuccess cosmosDb { get; set; } = new CosmosDbSuccess();
    }

    public class ErrorDetails
    {
        public CosmosDbError cosmosDb { get; set; } = new CosmosDbError();
    }

    public class CosmosDbSuccess
    {
        public string status { get; set; } = "UP";
        public HealthzSuccessDetails details { get; set; } = new HealthzSuccessDetails();
    }

    public class CosmosDbError
    {
        public string status { get; set; } = "DOWN";
        public HealthzErrorDetail details { get; set; } = new HealthzErrorDetail();
    }

    public class HealthzErrorDetail
    {
        public int Status { get; set; } = 503;
        public string Error { get; set; } = string.Empty;
    }

    public class HealthzSuccessDetails
    {
        public int Status { get; set; } = 200;
        public long Actors { get; set; }
        public long Movies { get; set; }
        public long Genres { get; set; }
        public string Instance { get; }
        public string Version { get {return Helium.Version.AssemblyVersion;} }
        public string CosmosKey { get; set; } = string.Empty;

        public HealthzSuccessDetails()
        {
            if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID")))
            {
                Instance = "unknown";
            }
            else
            {
                Instance = System.Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID");
            }
        }
    }
}
