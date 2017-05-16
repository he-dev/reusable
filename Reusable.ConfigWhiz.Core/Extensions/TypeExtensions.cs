using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reusable.ConfigWhiz.Data.Annotations;

namespace Reusable.ConfigWhiz.Extensions
{
    internal static class TypeExtensions
    {
        public static string GetCustomNameOrDefault(this MemberInfo property) => property.GetCustomAttribute<SettingNameAttribute>()?.ToString() ?? property.Name;
    }
}
