using CSE.Helium;
using System;

namespace Helium.Extensions
{
    public static class ActorExtensions
    {
        /// <summary>
        /// Add parameters to the method name if specified in the query string
        /// </summary>
        /// <param name="actorQueryParameters"></param>
        /// <returns></returns>
        public static string GetMethodText(this ActorQueryParameters actorQueryParameters)
        {
            _ = actorQueryParameters ?? throw new ArgumentNullException(nameof(actorQueryParameters));

            string method = "GetActorsAsync";

            // add the query parameters to the method name if exists
            if (!string.IsNullOrEmpty(actorQueryParameters.Q))
            {
                method = $"{method}:q:{actorQueryParameters.Q}";
            }
            
            if (actorQueryParameters.PageNumber != default)
            {
                method = $"{method}:pageNumber:{actorQueryParameters.PageNumber}";
            }

            if (actorQueryParameters.PageSize != default)
            {
                method = $"{method}:pageSize:{actorQueryParameters.PageSize}";
            }

            return method;
        }
    }
}
