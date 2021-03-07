#pragma warning disable CA1034 // Nested types should not be visible
using Prometheus;

using PM = Prometheus.Metrics;

namespace ValheimMods.Mod.MetricsExporter
{
    public static class Metrics
    {
        private static readonly Counter s_ticks = PM.CreateCounter("valheim_ticks_total", "Total update ticks since restart");
        public static ICounter Ticks => s_ticks;

        public static class Character
        {
            private static readonly Counter s_deaths = PM.CreateCounter("valheim_character_deaths_total", "Total death count of a character", Labels.Character.Name);
            public static ICounter Deaths(string character) => s_deaths.WithLabels(character);
        }

        public static class Plugin
        {
            private static readonly Gauge s_info = PM.CreateGauge("valheim_plugin_info", "Plugin information", Labels.Plugin.Guid, Labels.Plugin.Version);
            public static void Info(BepInEx.BepInPlugin metadata) => s_info.WithLabels(metadata.GUID, metadata.Version.ToString(3)).Set(1);
        }
    }
}
