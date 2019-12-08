using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace Helium.Model
{
    public class HealthzResult
    {
        public string Uri { get; set; }
        public HealthStatus Status { get; set; }
        public TimeSpan Duration { get; set; }
        public string Message { get; set; }
    }
}
