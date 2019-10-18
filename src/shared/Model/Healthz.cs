namespace Helium.Model
{
    public class HealthzSuccess
    {
        public string status = "UP";
        public SuccessDetails details = new SuccessDetails();
    }

    public class HealthzError
    {
        public string status = "DOWN";
        public ErrorDetails details = new ErrorDetails();
    }

    public class SuccessDetails
    {
        public CosmosDbSuccess cosmosDb = new CosmosDbSuccess();
    }

    public class ErrorDetails
    {
        public CosmosDbError cosmosDb = new CosmosDbError();
    }

    public class CosmosDbSuccess
    {
        public string status = "UP";
        public HealthzSuccessDetails details = new HealthzSuccessDetails();
    }

    public class CosmosDbError
    {
        public string status = "DOWN";
        public HealthzErrorDetail details = new HealthzErrorDetail();
    }

    public class HealthzErrorDetail
    {
        public int Status = 503;
        public string Error = string.Empty;
    }

    public class HealthzSuccessDetails
    {
        public int Status = 200;
        public long Actors;
        public long Movies;
        public long Genres;
        public readonly string Instance;
        public readonly string Version = Helium.Version.AssemblyVersion;

        public HealthzSuccessDetails()
        {
            if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID")))
            {
                Instance = "unknown";
            }
            else
<<<<<<< HEAD:src/shared/Model/Healthz.cs
            {
=======
            { 
>>>>>>> 1ee74d8537908f61a1f92432cc54855dd6889973:src/app/Model/Healthz.cs
                Instance = System.Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID");
            }
        }
    }
}
