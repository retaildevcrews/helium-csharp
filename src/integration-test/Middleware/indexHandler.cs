using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Helium;
using System;
using Newtonsoft.Json;

namespace Helium
{
    public static class HomePageMiddlewareExtensions
    {
        static readonly HashSet<string> validPaths = new HashSet<string> { "/", "/index.html", "/index.htm", "/default.html", "/default.htm" };

        /// <summary>
        /// Middleware extension method to handle home page request
        /// </summary>
        /// <param name="builder">this IApplicationBuilder</param>
        /// <returns></returns>
        public static IApplicationBuilder UseHomePage(this IApplicationBuilder builder)
        {
            const string queryKey = "maxage";

            // create the middleware
            builder.Use(async (context, next) =>
            {
                // matches / or index.htm[l] or default.htm[l]
                if (validPaths.Contains(context.Request.Path.Value.ToLower()))
                {
                    int maxAge = App.Metrics.MaxAge;

                    if (context.Request.Query.ContainsKey(queryKey))
                    {
                        if (int.TryParse(context.Request.Query[queryKey], out int val))
                        {
                            if (val > 0 && val < App.Metrics.MaxAge)
                            {
                                maxAge = val;
                            }
                        }
                    }

                    // build the response
                    string html = string.Format($"Helium Integration Test\nV {Helium.Version.AssemblyVersion}\n\n");
                    html += GetConfig();
                    html += GetRunningTime();
                    html += GetMetrics(maxAge);
                    html += GetHealthz().GetAwaiter().GetResult();

                    byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(html);

                    // return the content
                    context.Response.ContentType = "text/plain";
                    await context.Response.Body.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
                else
                {
                    // not a match, so call next middleware handler
                    await next();
                }
            });

            return builder;
        }

        /// <summary>
        /// call the /healtz endpoint
        /// </summary>
        /// <returns>string</returns>
        static async Task<string> GetHealthz()
        {
            string host = App._config.Host;

            string html = string.Format($"Healthz: {host}\n");

            // build the url
            if (!host.EndsWith("/"))
            {
                host += "/";
            }
            host += "healthz";

            try
            {
                // get the healthz results
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(host);
                string content = await response.Content.ReadAsStringAsync();

                // make the json pretty
                dynamic d = JsonConvert.DeserializeObject<dynamic>(content);
                content = JsonConvert.SerializeObject(d, Formatting.Indented);

                html += content;
            }
            catch (Exception ex)
            {
                html += ex.Message;
            }
            
            return html;
        }

        /// <summary>
        /// Compute the running time in minutes / hours / days
        /// </summary>
        /// <returns>string - Running for xx minutes</returns>
        static string GetRunningTime()
        {
            const string running = "Running for ";

            TimeSpan ts = DateTime.UtcNow.Subtract(App.StartTime);

            // 1 minute
            if (ts.TotalSeconds <= 90)
            {
                return running + "1 minute\n\n";
            }

            // xx minutes
            else if (ts.TotalMinutes <= 90)
            {
                return string.Format($"{running}{Math.Round(ts.TotalMinutes, 0)} minutes\n\n");
            }

            // xx hours
            else if (ts.TotalHours <= 36)
            {
                return string.Format($"{running}{Math.Round(ts.TotalHours, 0)} hours\n\n");
            }

            // xx.x days
            else
            {
                return string.Format($"{running}{Math.Round(ts.TotalDays, 1)} days\n\n");
            }
        }

        /// <summary>
        /// Get the metrics from the last 4 hours
        /// </summary>
        /// <returns>string</returns>
        static string GetMetrics(int maxAge)
        {
            // don't display metrics
            if (maxAge <= 0)
            {
                return string.Empty;
            }

            string html = "Metrics";

            if (DateTime.UtcNow.Subtract(App.StartTime).TotalMinutes > maxAge)
            {
                html += string.Format($" (prior {maxAge} minutes)");
            }

            html += "\n";

            var list = App.Metrics.GetMetricList(maxAge);
            foreach (var r in list)
            {
                html += string.Format($"{r.Key.PadLeft(4).PadRight(20).Substring(0,24)} {r.Count.ToString().PadLeft(12)}\n");
            }

            return html + "\n";
        }

        /// <summary>
        /// Get the current configuration
        /// </summary>
        /// <returns></returns>
        static string GetConfig()
        {
            string html = "Current Configuration: \n";

            html += string.Format($"\tThreads: {App._config.Threads}\n\tSleep: {App._config.SleepMs}\n");

            if (App._config.Random)
            {
                html += string.Format($"\tRandomize\n");
            }

            if (App._config.Verbose)
            {
                html += string.Format($"\tVerbose\n");
            }

            return html + "\n";
        }
    }
}
