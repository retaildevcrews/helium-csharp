using System.Collections.Generic;

namespace Helium.Model
{
    public class HealthzStatusCode
    {
        public const string Up = "UP";
        public const string Down = "DOWN";
        public const string Warn = "WARN";
    }

    public class HealthzStatus
    {
        public string StatusCode
        {
            get
            {
                if (!string.IsNullOrEmpty(Message))
                {
                    return HealthzStatusCode.Down;
                }

                string code = HealthzStatusCode.Up;

                foreach (var r in Results)
                {
                    if (r.StatusCode == HealthzStatusCode.Down)
                    {
                        return HealthzStatusCode.Down;
                    }

                    if (r.StatusCode == HealthzStatusCode.Warn)
                    {
                        code = HealthzStatusCode.Warn;
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
