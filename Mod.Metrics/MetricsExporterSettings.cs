using BepInEx.Configuration;
using ValheimMods.Common.Config;

namespace ValheimMods.Mod.MetricsExporter
{
    public class MetricsExporterSettings
    {
        [ConfigInt(Section = "General", Key = "Port", Default = 9869, Description = "Port on which metrics will be served (only applicable when not using Pushgateway)")]
        public ConfigEntry<int> Port { get; set; }

        [BindConfig]
        public PushgatewaySettings Pushgateway { get; set; }
    }

    public class PushgatewaySettings
    {
        [ConfigBool(Section = "General.Pushgateway", Key = "Enabled", Default = false, Description = "Use Prometheus Pushgateway in case you are not allowed to start HTTP services on custom ports")]
        public ConfigEntry<bool> Enabled { get; set; }

        [ConfigString(Section = "General.Pushgateway", Key = "Endpoint", Default = "https://pushgateway.localhost/metrics/", Description = "Pushgateway endpoint")]
        public ConfigEntry<string> Endpoint { get; set; }

        [ConfigString(Section = "General.Pushgateway", Key = "Job", Default = "valheim", Description = "Name of the job")]
        public ConfigEntry<string> Job { get; set; }

        [ConfigString(Section = "General.Pushgateway", Key = "Instance", Default = "server01", Description = "Name of the instance")]
        public ConfigEntry<string> Instance { get; set; }

        [ConfigBool(Section = "General.Pushgateway", Key = "SkipTlsVerify", Default = false, Description = "Don't verify endpoint's certificate")]
        public ConfigEntry<bool> SkipTlsVerify { get; set; }
    }
}
