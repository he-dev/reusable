using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Data.Annotations;

namespace Reusable.Commander
{
    public static class CommandHelper
    {
        private static readonly ConcurrentDictionary<Type, NameSet> CommandNameCache = new ConcurrentDictionary<Type, NameSet>();

        private static readonly ConcurrentDictionary<PropertyInfo, NameSet> ParameterNameCache = new ConcurrentDictionary<PropertyInfo, NameSet>();

        [NotNull]
        public static NameSet GetCommandId([NotNull] Type commandType)
        {
            if (commandType == null) throw new ArgumentNullException(nameof(commandType));

            if (!typeof(ICommand).IsAssignableFrom(commandType))
            {
                throw new ArgumentException(
                    paramName: nameof(commandType),
                    message: $"'{nameof(commandType)}' needs to be derived from '{nameof(System.Windows.Input.ICommand)}'");
            }

            return CommandNameCache.GetOrAdd(commandType, t =>
            {
                //var category = t.GetCustomAttribute<CategoryAttribute>()?.Category;

                return new NameSet(GetCommandNames(t).Distinct());

                // return new Identifier(
                //     category is null
                //         ? names
                //         : names.SelectMany(name => new[] {name, SoftString.Create($"{category}.{name}")})
                // );
            });
        }

        private static IEnumerable<Name> GetCommandNames(Type commandType)
        {
            yield return new Name(GetDefaultCommandName(commandType), NameOption.Default);
            foreach (var name in commandType.GetCustomAttribute<TagsAttribute>() ?? Enumerable.Empty<string>())
            {
                yield return new Name(name, NameOption.Alias);
            }
        }

        private static SoftString GetDefaultCommandName(Type commandType)
        {
            return Regex.Replace(commandType.Name, "C(omman|m)d$", string.Empty, RegexOptions.IgnoreCase);
        }

        [NotNull]
        public static NameSet GetCommandParameterId([NotNull] PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            return ParameterNameCache.GetOrAdd(property, p =>
            {
                var names = GetParameterNames(p);
                return new NameSet(names);
            });
        }

        public static IEnumerable<Name> GetParameterNames(MemberInfo property)
        {
            // Always use the property name as default.
            yield return new Name(property.Name, NameOption.Default);

            // Then get alias if any.
            foreach (var alias in property.GetCustomAttribute<TagsAttribute>() ?? Enumerable.Empty<string>())
            {
                yield return new Name(alias, NameOption.Alias);
            }
        }

        public static IEnumerable<CommandArgumentMetadata> GetCommandArgumentMetadata(this Type commandArgumentGroupType)
        {
            return
                commandArgumentGroupType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => !p.IsDefined(typeof(NotMappedAttribute)))
                    .Select(CommandArgumentMetadata.Create);
        }

        public static Type GetCommandArgumentGroupType(this Type commandType)
        {
            if (commandType.BaseType == typeof(SimpleCommand))
            {
                return typeof(ICommandLine);
            }

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