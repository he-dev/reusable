using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Reusable.DoubleDash.Annotations;
using Reusable.Marbles.Annotations;
using Reusable.Marbles.Extensions;
using Reusable.Utilities.Autofac;

namespace Reusable.DoubleDash;

public interface ICommandNameResolver
{
    NameCollection ResolveCommandName<T>() where T : ICommand;
}

public static class CommandHelper
{
    private static readonly ConcurrentDictionary<MemberInfo, NameCollection> NameCache = new ConcurrentDictionary<MemberInfo, NameCollection>();

    public static NameCollection GetArgumentName(this MemberInfo member)
    {
        return NameCache.GetOrAdd(member, t => new NameCollection(GetDefaultMemberName(t), t.GetCustomAttribute<AliasAttribute?>() ?? Enumerable.Empty<string>()));
    }
        
    private static string GetDefaultMemberName(MemberInfo commandType)
    {
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