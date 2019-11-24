using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Data.Annotations;

namespace Reusable.Commander
{
    public static class CommandHelper
    {
        private static readonly ConcurrentDictionary<MemberInfo, MultiName> NameCache = new ConcurrentDictionary<MemberInfo, MultiName>();
        
        public static MultiName GetMultiName(MemberInfo member)
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

        public static Type GetCommandArgumentGroupType(this Type commandType)
        {
//            if (commandType.BaseType == typeof(ICommand))
//            {
//                return typeof(ICommandLine);
//            }

            // ReSharper disable once PossibleNullReferenceException
            // The validation makes sure that this is never null.                        
            return
                commandType
                    .BaseType
                    .GetGenericArguments()
                    .Single(t => typeof(ICommandLine).IsAssignableFrom(t));
        }
    }
}