using System.Collections.Generic;

namespace Helium.Model
{
    public class HealthzStatusCode
    {
        // TODO - convert to use HealthStatus enum
        public const string Healthy = "Healthy";
        public const string Unhealthy = "Unhealthy";
        public const string Degraded = "Degraded";
    }

    public class HealthzStatus
    {
        public string StatusCode
        {
            get
            {
                if (!string.IsNullOrEmpty(Message))
                {
                    return HealthzStatusCode.Unhealthy;
                }

                string code = HealthzStatusCode.Healthy;

                foreach (var r in Results)
                {
                    if (r.StatusCode == HealthzStatusCode.Unhealthy)
                    {
                        return HealthzStatusCode.Unhealthy;
                    }

                    if (r.StatusCode == HealthzStatusCode.Degraded)
                    {
                        code = HealthzStatusCode.Degraded;
                    }
                }

                return code;
            }
        }
        public string Message { get; set; }

        public long TotalMilliseconds
        {
            get
            {
                long res = 0;

                foreach (var r in Results)
                {
                    res += r.TotalMilliseconds;
                }

                return res;
            }
        }

        public string Instance { get; } = "unknown";
        public string Version { get { return Helium.Version.AssemblyVersion; } }
        public string CosmosKey { get; set; } = string.Empty;

        public List<HealthzResult> Results { get; } = new List<HealthzResult>();

        public HealthzStatus()
        {
            if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID")))
            {
                Instance = System.Environment.GetEnvironmentVariable("WEBSITE_ROLE_INSTANCE_ID");
            }
        }
    }

    public class HealthzResult
    {
        public string Uri { get; set; }
        public string StatusCode { get; set; }
        public long TotalMilliseconds { get; set; }
        public string Message { get; set; }
    }
}
