#pragma warning disable CA1034 // Nested types should not be visible

namespace ValheimMods.Mod.MetricsExporter
{
    public static class Labels
    {
        public static class Character
        {
            public static string Name => "name";
        }

        public static class Plugin
        {
            public static string Guid => "guid";
            public static string Version => "version";
        }
    }
}
