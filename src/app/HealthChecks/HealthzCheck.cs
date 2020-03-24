using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Globalization;

namespace Helium.Model
{
    /// <summary>
    /// Health Check that supports dotnet IHeathCheck
    /// </summary>
    public class HealthzCheck
    {
        public HealthStatus Status { get; set; }
        public string ComponentId { get; set; }
        public string ComponentType { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan TargetDuration { get; set; }
        public string Time { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        public string Endpoint { get; set; }
        public string Message { get; set; }

        public const string TimeoutMessage = "Request exceeded expected duration";
    }
}
