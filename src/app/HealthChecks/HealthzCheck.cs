using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace Helium.Model
{
    /// <summary>
    /// Health Check that supports dotnet IHeathCheck
    /// </summary>
    public class HealthzCheck
    {
        public HealthStatus Status { get; set; }
        public TimeSpan Duration { get; set; }
        public string Time => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        public string Uri { get; set; }
        public string Message { get; set; }
    }
}
