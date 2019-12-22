using System;
using System.Collections.Generic;
using System.Linq;

namespace Helium
{
    public sealed class Metrics
    {
        public int MaxAge { get; set; } = 240;

        public List<Metric> Requests { get; } = new List<Metric>();

        /// <summary>
        /// Remove old entries to keep the list from growing boundless
        /// </summary>
        public void Prune()
        {
            if (MaxAge <= 0)
            {
                // don't track metrics
                lock (Requests)
                {
                    Requests.Clear();
                }
            }
            else
            {
                // keep MaxAge minutes of metrics
                DateTime now = DateTime.UtcNow.AddMinutes(-1 * MaxAge);

                // remove the first item until the date is out of range
                // the list is not 100% sorted but there is a where on the query, so a few extra will be ignored until next Prune
                lock (Requests)
                {
                    while (Requests.Count > 0 && Requests[0].Time < now)
                    {
                        Requests.RemoveAt(0);
                    }
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
                new MetricAggregate { Key = "Total Requests" },
                new MetricAggregate { Key = "2xx" },
                new MetricAggregate { Key = "3xx" },
                new MetricAggregate { Key = "4xx" },
                new MetricAggregate { Key = "5xx" },
                new MetricAggregate { Key = "Validation Errors" }
            };

            DateTime minDate = DateTime.UtcNow.AddMinutes(-1 * maxAge);

            // run the aggregate query
            lock (Requests)
            {
                var query = Requests.Where(r => r.Time >= minDate)
                    .GroupBy(r => r.Key,
                    (key, reqs) => new
                    {
                        Key = key,
                        Count = reqs.Count(),
                        Duration = reqs.Sum(d => d.Duration),
                        Min = reqs.Min(d => d.Duration),
                        Max = reqs.Max(d => d.Duration)
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
                            m3.Min = r.Min;
                            m3.Max = r.Max;
                            m3.Average = m3.Count > 0 ? m3.Duration / m3.Count : 0;
                            break;
                        }
                    }
                }
            }

            // set min value high
            res[0].Min = 256 * 1024;

            // sum the 2xx, 3xx, 4xx, 5xx for total results
            for (int i = 1; i < 5; i++)
            {
                res[0].Count += res[i].Count;
                res[0].Duration += res[i].Duration;

                res[0].Average = res[0].Count > 0 ? res[0].Duration / res[0].Count : 0;
                res[0].Min = res[i].Min > 0 && res[0].Min > res[i].Min ? res[i].Min : res[0].Min;
                res[0].Max = res[0].Max < res[i].Max ? res[i].Max : res[0].Max;
            }

            return res;
        }

        /// <summary>
        /// Get the metric key from the status code
        /// </summary>
        /// <param name="status"></param>
        /// <returns>2xx, 3xx, etc.</returns>
        public static string GetKeyFromStatus(int status)
        {
            switch (status / 100)
            {
                case 0:
                    return "Validation Errors";
                case 2:
                case 3:
                case 4:
                case 5:
                    return string.Format(System.Globalization.CultureInfo.InvariantCulture, $"{status / 100}xx");

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
                    lock (Requests)
                    {
                        Requests.Add(new Metric { Key = GetKeyFromStatus(status), Duration = duration });
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents one request
    /// </summary>
    public class Metric
    {
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public string Key { get; set; } = string.Empty;
        public long Duration { get; set; } = 0;
    }

    /// <summary>
    /// Metric aggregation by Key
    /// </summary>
    public class MetricAggregate
    {
        public string Key { get; set; } = string.Empty;
        public long Count { get; set; } = 0;
        public long Duration { get; set; } = 0;
        public double Average { get; set; } = 0;
        public long Min { get; set; } = 0;
        public long Max { get; set; } = 0;
    }
}
