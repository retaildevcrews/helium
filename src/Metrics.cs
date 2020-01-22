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
            List<MetricAggregate> res = new List<MetricAggregate>();

            DateTime minDate = DateTime.UtcNow.AddMinutes(-1 * maxAge);

            List<dynamic> query;

            // run the aggregate query
            lock (Requests)
            {
                query = Requests.Where(r => r.Time >= minDate)
                    .GroupBy(r => r.Category,
                    (cat, reqs) => new
                    {
                        Category = cat,
                        Count = reqs.Count(),
                        Failures = reqs.Count(d => d.StatusCode >= 400),
                        ValidationErrors = reqs.Count(d => !d.Validated),
                        Q1 = reqs.Count(d => d.PerfLevel == 1),
                        Q2 = reqs.Count(d => d.PerfLevel == 2),
                        Q3 = reqs.Count(d => d.PerfLevel == 3),
                        Q4 = reqs.Count(d => d.PerfLevel == 4),
                        Duration = reqs.Sum(d => d.Duration),
                        Min = reqs.Min(d => d.Duration),
                        Max = reqs.Max(d => d.Duration)
                    }).OrderBy(d => d.Category).ToList<dynamic>();
            }

            MetricAggregate m3;

            // update the result list based on the aggregate
            foreach (var r in query)
            {
                m3 = new MetricAggregate();
                res.Add(m3);

                m3.Category = r.Category;
                m3.Count = r.Count;
                m3.Failures = r.Failures;
                m3.ValidationErrors = r.ValidationErrors;
                m3.Q1 = r.Q1;
                m3.Q2 = r.Q2;
                m3.Q3 = r.Q3;
                m3.Q4 = r.Q4;
                m3.Duration = r.Duration;
                m3.Min = r.Min;
                m3.Max = r.Max;
                m3.Average = m3.Count > 0 ? m3.Duration / m3.Count : 0;
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
        /// <param name="category">category of request</param>
        /// <param name="perfLevel">perf level (quartile)</param>
        /// <param name="validated">validated successfully</param>
        public void Add(int status, double duration, string category, bool validated, int perfLevel)
        {
            // only track if MaxAge > 0
            if (MaxAge > 0)
            {
                // validate status
                if (status >= 200 && status < 600)
                {
                    lock (Requests)
                    {
                        Requests.Add(new Metric { StatusCode = status, Duration = duration, Category = category, Validated = validated, PerfLevel = perfLevel });
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
        public int StatusCode { get; set; } = 0;
        public double Duration { get; set; } = 0;
        public string Category { get; set; } = string.Empty;
        public int PerfLevel { get; set; } = 0;
        public bool Validated { get; set; } = true;
    }

    /// <summary>
    /// Metric aggregation by Category
    /// </summary>
    public class MetricAggregate
    {
        public string Category { get; set; }
        public int Total { get; set; }
        public int Failures { get; set; }
        public int ValidationErrors { get; set; }
        public int Q1 { get; set; }
        public int Q2 { get; set; }
        public int Q3 { get; set; }
        public int Q4 { get; set; }
        public long Count { get; set; } = 0;
        public double Duration { get; set; } = 0;
        public double Average { get; set; } = 0;
        public double Min { get; set; } = 0;
        public double Max { get; set; } = 0;
    }
}
