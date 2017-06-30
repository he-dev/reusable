using System.Reflection;
using Reusable.SmartConfig.Annotations;

namespace Reusable.SmartConfig.Extensions
{
    internal static class TypeExtensions
    {
        public static string GetCustomNameOrDefault(this MemberInfo property) => property.GetCustomAttribute<SettingNameAttribute>()?.ToString() ?? property.Name;
    }
}
