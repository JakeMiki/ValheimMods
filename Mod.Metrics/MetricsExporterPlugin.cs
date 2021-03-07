using BepInEx;
using Prometheus;
using BepInEx.Logging;
using ValheimMods.Common.Config;
using ValheimMods.Mod.MetricsExporter.Servers;

namespace ValheimMods.Mod.MetricsExporter
{
    [BepInPlugin("com.github.jakemiki.valheim-mods.metrics-exporter", "Valheim Metrics Exporter", "1.0.0")]
    public class MetricsExporterPlugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; }
        public static MetricsExporterSettings Settings { get; private set; }

        private IMetricServer _metricServer;

        protected void Awake()
        {
            Log.LogDebug("Binding settings");
            Settings = Config.Bind<MetricsExporterSettings>();
            Log = Logger;

            Log.LogDebug("Exporting plugins' metadata metrics");
            foreach (var plugin in FindObjectsOfType<BaseUnityPlugin>())
            {
                Metrics.Plugin.Info(MetadataHelper.GetMetadata(plugin));
            }
        }

        protected void OnEnable()
        {
            Log.LogInfo("Starting Prometheus Metrics Exporter");
            _metricServer = CreateMetricServer().Start();
        }
        protected void OnDisable()
        {
            Log.LogInfo("Stopping Prometheus Metrics Exporter");
            _metricServer.Stop();
            _metricServer = null;
        }

        protected void Update()
        {
            Metrics.Ticks.Inc();
        }

        private IMetricServer CreateMetricServer()
        {
            if (!Settings.Pushgateway.Enabled.Value)
            {
                _metricServer = new MetricServer(port: Settings.Port.Value);
            }
            else
            {
                var pushgateway = Settings.Pushgateway;
                _metricServer = new UnityMetricPusher(this, Log, new UnityMetricPusherOptions
                {
                    Endpoint = pushgateway.Endpoint.Value,
                    Job = pushgateway.Job.Value,
                    Instance = pushgateway.Instance.Value,
                    SkipTlsVerify = pushgateway.SkipTlsVerify.Value,
                });
            }

            return _metricServer;
        }
    }
}
