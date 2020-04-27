using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace CSE.Helium
{
    public static class CosmosHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddCosmosHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null)
        {
            // Register a check of type Cosmos
            builder.AddCheck<CosmosHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            return builder;
        }
    }
}
