using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Reusable.Commander.Annotations;
using Reusable.Data.Annotations;
using Reusable.Extensions;

namespace Reusable.Commander
{
    public interface ICommandNameResolver
    {
        ArgumentNameCollection ResolveCommandName<T>() where T : ICommand;
    }

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
            if (commandType.IsAssignableFrom(typeof(ICommand))) throw new ArgumentException($"'{nameof(commandType)}' must by of type '{typeof(ICommand).ToPrettyString()}'.");

            do
            {
                if (commandType.IsGenericType && commandType.GetGenericTypeDefinition() == typeof(Command<>))
                {
                    return commandType.GetGenericArguments().Single();
                }
                else
                {
                    commandType = commandType.BaseType;
                }
            } while (commandType is {});

            return typeof(object);
        }

        //public static Type ParameterType(this ICommand command) => command.GetType().GetCommandParameterType();

        public static IEnumerable<PropertyInfo> GetParameterProperties(this Type parameterType)
        {
            return
                from p in parameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where !p.IsDefined(typeof(NotMappedAttribute)) && !p.IsDefined(typeof(ServiceAttribute))
                select p;
        }
    }
}