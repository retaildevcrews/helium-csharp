using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace Helium.Model
{
    /// <summary>
    /// Health Check that supports IETF json
    /// </summary>
    public class IetfCheck
    {
        public string Status { get; set; }
        public string ComponentType { get; set; }
        public double ObservedValue { get; set; }
        public string ObservedUnit { get; set; }
        public string Time { get; set; }
        public string Uri { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public IetfCheck() { }

        /// <summary>
        /// Create an IetfCheck from a HealthzCheck
        /// </summary>
        /// <param name="hzCheck">HealthzCheck</param>
        public IetfCheck(HealthzCheck hzCheck)
        {
            Status = ToIetfStatus(hzCheck.Status);
            ComponentType = "CosmosDB";
            ObservedValue = Math.Round(hzCheck.Duration.TotalMilliseconds, 2);
            ObservedUnit = "ms";
            Time = hzCheck.Time;
            Uri = hzCheck.Uri;
            Message = hzCheck.Message;
        }

        /// <summary>
        /// Convert the dotnet HealthStatus to the IETF Status
        /// </summary>
        /// <param name="status">HealthStatus (dotnet)</param>
        /// <returns>string</returns>
        public static string ToIetfStatus(HealthStatus status)
        {
            return status switch
            {
                HealthStatus.Healthy => "up",
                HealthStatus.Degraded => "warn",
                _ => "down"
            };
        }
    }
}
