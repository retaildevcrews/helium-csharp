using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Helium
{
    public class Metrics
    {
        public int MaxAge = 240;

        public readonly List<Metric> Requests = new List<Metric>();

        /// <summary>
        /// Remove old entries to keep the list from growing boundless
        /// </summary>
        public void Prune()
        {
            if (MaxAge <= 0)
            {
                // don't track metrics
                Requests.Clear();
            }
            else
            {
                // keep MaxAge minutes of metrics
                DateTime now = DateTime.UtcNow.AddMinutes(-1 * MaxAge);

                // remove the first item until the date is out of range
                // the list is not 100% sorted but there is a where on the query, so a few extra will be ignored until next Prune
                while (Requests.Count > 0 && Requests[0].Time < now)
                {
                    Requests.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Get the metric aggregates
        /// </summary>
        /// <returns>List of MetricAggregate</returns>
        public List<MetricAggregate> GetMetricList(int maxAge)
        {
            // Build the list of expected results
            List<MetricAggregate> res = new List<MetricAggregate>
            {
                new MetricAggregate { Key = "Requests" },
                new MetricAggregate { Key = "2xx" },
                new MetricAggregate { Key = "3xx" },
                new MetricAggregate { Key = "4xx" },
                new MetricAggregate { Key = "5xx" },
                new MetricAggregate { Key = "Validation Errors" }
            };

            DateTime minDate = DateTime.UtcNow.AddMinutes(-1 * maxAge);

            // run the aggregate query
            var query = Requests.Where(r => r.Time >= minDate)
                .GroupBy(
                r => r.Key,
                (key, reqs) => new
                {
                    Key = key,
                    Count = reqs.Count(),
                    Duration = reqs.Sum(d => d.Duration)
                });

            // update the result list based on the aggregate
            foreach (var r in query)
            {
                foreach (var m3 in res)
                {
                    if (m3.Key == r.Key)
                    {
                        m3.Count = r.Count;
                        m3.Duration = r.Duration;
                        break;
                    }
                }
            }

            // sum the 2xx, 3xx, 4xx, 5xx for total results
            for (int i = 1; i < 5; i++)
            {
                res[0].Count += res[i].Count;
            }

            return res;
        }

        /// <summary>
        /// Get the metric key from the status code
        /// </summary>
        /// <param name="status"></param>
        /// <returns>2xx, 3xx, etc.</returns>
        public string GetKeyFromStatus(int status)
        {
            switch (status / 100)
            {
                case 0:
                    return "Validation Errors";
                case 2:
                case 3:
                case 4:
                case 5:
                    return string.Format($"{status / 100}xx");

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Add a metric to the list
        /// </summary>
        /// <param name="status">http status code (or 0 for validation error)</param>
        /// <param name="duration">duration of request in ms</param>
        public void Add(int status, int duration)
        {
            // only track if MaxAge > 0
            if (MaxAge > 0)
            {
                // validate status
                if (status == 0 || (status >= 200 && status < 600))
                {
                    Requests.Add(new Metric { Key = GetKeyFromStatus(status), Duration = duration });
                }
            }
        }
    }

    /// <summary>
    /// Represents one request
    /// </summary>
    public class Metric
    {
        public DateTime Time = DateTime.UtcNow;
        public string Key = string.Empty;
        public long Duration = 0;
    }

    /// <summary>
    /// Metric aggregation by Key
    /// </summary>
    public class MetricAggregate
    {
        public string Key = string.Empty;
        public long Count = 0;
        public long Duration = 0;
    }
}
