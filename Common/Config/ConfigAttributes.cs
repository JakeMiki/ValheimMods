using System.Reflection;
using BepInEx.Configuration;

namespace ValheimMods.Common.Config
{
    public sealed class ConfigIntAttribute : BaseConfigAttribute
    {
        public int Default { get; set; }

        public override void Bind(object target, MemberInfo memberInfo, ConfigFile configFile)
        {
            memberInfo.SetPropertyOrFieldValue(target, BindConfig(configFile, Default));
        }
    }

    public sealed class ConfigStringAttribute : BaseConfigAttribute
    {
        public string Default { get; set; }

        public override void Bind(object target, MemberInfo memberInfo, ConfigFile configFile)
        {
            memberInfo.SetPropertyOrFieldValue(target, BindConfig(configFile, Default));
        }
    }

    public sealed class ConfigBoolAttribute : BaseConfigAttribute
    {
        public bool Default { get; set; }

        public override void Bind(object target, MemberInfo memberInfo, ConfigFile configFile)
        {
            memberInfo.SetPropertyOrFieldValue(target, BindConfig(configFile, Default));
        }
    }
}
