using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

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
