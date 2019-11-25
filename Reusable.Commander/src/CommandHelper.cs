using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Data.Annotations;
using Reusable.Extensions;

namespace Reusable.Commander
{
    public static class CommandHelper
    {
        private static readonly ConcurrentDictionary<MemberInfo, MultiName> NameCache = new ConcurrentDictionary<MemberInfo, MultiName>();

        public static MultiName GetMultiName(this MemberInfo member)
        {
            return NameCache.GetOrAdd(member, t => new MultiName(GetMemberNames(t)));
        }

        private static IEnumerable<string> GetMemberNames(MemberInfo commandType)
        {
            yield return GetDefaultMemberName(commandType);
            foreach (var name in commandType.GetCustomAttribute<AliasAttribute>() ?? Enumerable.Empty<string>())
            {
                yield return name;
            }
        }

        private static string GetDefaultMemberName(MemberInfo commandType)
        {
            // todo - make this clean-up optional
            return Regex.Replace(commandType.Name, "C(omman|m)d$", string.Empty, RegexOptions.IgnoreCase);
        }

        public static Type GetCommandParameterType(this Type commandType)
        {
            if (typeof(ICommand).IsAssignableFrom(commandType)) throw new ArgumentException($"'{nameof(commandType)}' must by of type '{typeof(ICommand).ToPrettyString()}'.");

            return
                commandType
                    .GetGenericArguments()
                    .SingleOrDefault() ?? typeof(object);
        }
        
        public static IEnumerable<PropertyInfo> GetParameterProperties(this Type parameterType)
        {
            return
                from p in parameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where !p.IsDefined(typeof(NotMappedAttribute)) && !p.IsDefined(typeof(ServiceAttribute))
                select p;
        }
    }
}