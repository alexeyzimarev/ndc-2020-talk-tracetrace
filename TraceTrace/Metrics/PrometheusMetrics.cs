using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Prometheus;

namespace TraceTrace.Metrics
{
    public static class PrometheusMetrics
    {
        static bool   isConfigured;
        static string appName;

        internal static IHistogram DbUpdateTimer(string collection) => dbUpdateTimer.Labels(appName, collection);

        public static ICounter DBUpdateErrorCounter(string collection) => dbUpdateErrorCounter.Labels(appName, collection);

        internal static void TryConfigure(string applicationName)
        {
            if (isConfigured) return;

            var bounds = new[]
                {.002, .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10, 30, 60, 120, 180, 240, 300};

            appName = applicationName;

            dbUpdateTimer = Prometheus.Metrics.CreateHistogram(
                "app_db_update_time_seconds",
                "Time to execute an update",
                new HistogramConfiguration
                {
                    Buckets    = bounds,
                    LabelNames = new[] {"app_name", "collection"}
                }
            );

            dbUpdateErrorCounter = Prometheus.Metrics.CreateCounter(
                "app_db_update_error_count",
                "Number of errors during database update",
                new[] {"app_name", "collection"}
            );

            isConfigured = true;
        }
        
        public static async Task Measure(Func<Task> action, IHistogram metric, ICounter errorCounter = null)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                await action();
            }
            catch (Exception)
            {
                errorCounter?.Inc();
                throw;
            }
            finally
            {
                stopwatch.Stop();
                metric.Observe(stopwatch.ElapsedTicks / (double) Stopwatch.Frequency);
            }
        }


        static Histogram dbUpdateTimer;
        static Counter   dbUpdateErrorCounter;
    }
}
