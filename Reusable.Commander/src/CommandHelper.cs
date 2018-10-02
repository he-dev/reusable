using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Extensions;

namespace Reusable.Commander
{
    public static class CommandHelper
    {
        private static readonly ConcurrentDictionary<Type, Identifier> CommandNameCache = new ConcurrentDictionary<Type, Identifier>();

        private static readonly ConcurrentDictionary<PropertyInfo, Identifier> ParameterNameCache = new ConcurrentDictionary<PropertyInfo, Identifier>();

        [NotNull]
        public static Identifier GetCommandId([NotNull] Type commandType)
        {
            if (commandType == null) throw new ArgumentNullException(nameof(commandType));

            if (!typeof(IConsoleCommand).IsAssignableFrom(commandType))
            {
                throw new ArgumentException(
                    paramName: nameof(commandType),
                    message: $"'{nameof(commandType)}' needs to be derived from '{nameof(ICommand)}'");
            }

            return CommandNameCache.GetOrAdd(commandType, t =>
            {
                var category = t.GetCustomAttribute<CategoryAttribute>()?.Category;

                var names = GetCommandNames(t).Distinct();

                return new Identifier(
                    category is null
                        ? names
                        : names.SelectMany(name => new[] {name, SoftString.Create($"{category}.{name}")})
                );
            });
        }

        private static IEnumerable<SoftString> GetCommandNames(Type commandType)
        {
            yield return GetDefaultCommandName(commandType);
            foreach (var name in commandType.GetCustomAttribute<AliasAttribute>() ?? Enumerable.Empty<SoftString>())
            {
                yield return name;
            }
        }

        private static SoftString GetDefaultCommandName(Type commandType)
        {
            return Regex.Replace(commandType.Name, "C(omman|m)d$", string.Empty, RegexOptions.IgnoreCase);
        }

        [NotNull]
        public static Identifier GetCommandParameterId([NotNull] PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            return ParameterNameCache.GetOrAdd(property, p =>
            {
                var names = GetParameterNames(p);
                return new Identifier(names);
            });
        }

        private static IEnumerable<SoftString> GetParameterNames(PropertyInfo property)
        {
            // Always use the property name as default.
            yield return property.Name;

            // Then get alias if any.
            foreach (var alias in property.GetCustomAttribute<AliasAttribute>() ?? Enumerable.Empty<SoftString>())
            {
                yield return alias;
            }
        }

        public static IEnumerable<CommandParameter> GetParameters(this Type bagType)
        {
            return
                bagType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => !p.IsDefined(typeof(NotMappedAttribute)))
                    .Select(CommandParameter.Create);
        }

        public static Type GetBagType(this Type commandType)
        {
            CommandValidator.ValidateCommandType(commandType);

            // ReSharper disable once PossibleNullReferenceException
            // The validation makes sure that this is never null.
            return
                commandType
                    .BaseType
                    .GetGenericArguments()
                    .Single(t => typeof(ICommandBag).IsAssignableFrom(t));
        }        
    }
}