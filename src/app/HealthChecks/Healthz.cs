using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace Helium.Model
{
    public class HealthzCheck
    {
        public HealthStatus Status { get; set; }
        public TimeSpan Duration { get; set; }
        public string Time => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        public string Uri { get; set; }
        public string Message { get; set; }
    }

    public class IetfCheck
    {
        public string Status { get; set; }
        public string ComponentType { get; set; }
        public double ObservedValue { get; set; }
        public string ObservedUnit { get; set; }
        public string Time { get; set; }
        public string Uri { get; set; }
        public string Message { get; set; }
    }
}
