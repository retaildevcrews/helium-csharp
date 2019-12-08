using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace Helium.Model
{
    public class HealthzResult
    {
        public string Uri { get; set; }
        public HealthStatus StatusCode { get; set; }
        public long TotalMilliseconds { get; set; }
        public string Message { get; set; }
    }
}
