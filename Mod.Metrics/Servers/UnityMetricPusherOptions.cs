using System;
using System.Collections.Generic;
using Prometheus;

namespace ValheimMods.Mod.MetricsExporter.Servers
{
    public class UnityMetricPusherOptions
    {
        internal static readonly UnityMetricPusherOptions Default = new UnityMetricPusherOptions();

        public string Endpoint { get; set; }
        public string Job { get; set; }
        public string Instance { get; set; }
        public long IntervalMilliseconds { get; set; } = 1000;
        public IEnumerable<Tuple<string, string>> AdditionalLabels { get; set; }
        public bool SkipTlsVerify { get; set; }
        public CollectorRegistry Registry { get; set; }

        /// <summary>
        /// Callback for when a metric push fails.
        /// </summary>
        public Action<string> OnError { get; set; }

        /// <summary>
        /// Provider for UnityWebRequest in which you can handle authorization etc.
        /// </summary>
        public UnityWebRequestProvider UnityWebRequestProvider { get; set; }
    }
}
