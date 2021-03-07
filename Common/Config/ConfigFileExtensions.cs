using System;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace ValheimMods.Common.Config
{
    public static class ConfigFileExtensions
    {
        public static T Bind<T>(this ConfigFile configFile) where T : new()
        {
            var target = new T();
            configFile.Bind(target);
            return target;
        }

        public static object Bind(this ConfigFile configFile, Type type)
        {
            var target = Activator.CreateInstance(type);
            configFile.Bind(target);
            return target;
        }

        public static void Bind(this ConfigFile configFile, object target)
        {
            var type = target.GetType();
            var logger = Logger.CreateLogSource($"ConfigFileExtensions.Bind(\"{type.FullName}\")");

            foreach (var memberInfo in type.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                var attribute = memberInfo.GetConfigAttribute();
                logger.LogDebug($"Checking member {memberInfo.Name}");
                if (attribute != null)
                {
                    logger.LogDebug($"Binding config {attribute.FullPath} to member {memberInfo.Name}");
                    attribute.Bind(target, memberInfo, configFile);
                }
                else if (memberInfo.GetCustomAttribute<BindConfigAttribute>() != null)
                {
                    var memberType = memberInfo.GetPropertyOrFieldType();
                    if (memberType != null)
                    {
                        logger.LogDebug($"Binding configs to member {memberInfo.Name} of type {type.FullName}");
                        memberInfo.SetPropertyOrFieldValue(target, configFile.Bind(memberType));
                    }
                }
            }

            logger.Dispose();
        }

        public static void SetPropertyOrFieldValue(this MemberInfo memberInfo, object target, object value)
        {
            if (memberInfo.MemberType == MemberTypes.Property)
            {
                (memberInfo as PropertyInfo).SetValue(target, value);
            }
            else if (memberInfo.MemberType == MemberTypes.Field)
            {
                (memberInfo as FieldInfo).SetValue(target, value);
            }
        }

        public static Type GetPropertyOrFieldType(this MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Property)
            {
                return (memberInfo as PropertyInfo).PropertyType;
            }
            else if (memberInfo.MemberType == MemberTypes.Field)
            {
                return (memberInfo as FieldInfo).FieldType;
            }
            return null;
        }

        private static BaseConfigAttribute GetConfigAttribute(this MemberInfo memberInfo)
        {
            return memberInfo
                    .GetCustomAttributes()
                    .Where(attr => typeof(BaseConfigAttribute).IsAssignableFrom(attr.GetType()))
                    .Cast<BaseConfigAttribute>()
                    .FirstOrDefault();
        }
    }
}
