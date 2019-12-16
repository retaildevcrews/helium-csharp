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
        public string ComponentType { get; set; }
        public TimeSpan Duration { get; set; }
        public string Time => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        public string Endpoint { get; set; }
        public string Message { get; set; }
    }
}
