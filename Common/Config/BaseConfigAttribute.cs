#pragma warning disable CA1813 // Avoid unsealed attributes
using System;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;

namespace ValheimMods.Common.Config
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public abstract class BaseConfigAttribute : Attribute
    {
        public string Section { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public AcceptableValueBase AcceptableValues { get; set; }

        public string FullPath => $"{Section}.{Key}";

        public abstract void Bind(object target, MemberInfo fieldInfo, ConfigFile configFile);

        protected ConfigEntry<T> BindConfig<T>(ConfigFile configFile, T defaultValue)
        {
            var description = (!string.IsNullOrEmpty(Description) || AcceptableValues != null) ? new ConfigDescription(Description, AcceptableValues) : ConfigDescription.Empty;
            return configFile.Bind(Section, Key, defaultValue, description);
        }
    }
}
