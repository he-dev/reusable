using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Reusable.Fuse;

namespace Reusable.Shelly.Reflection
{
    internal static class CommandExtensions
    {
        public static IEnumerable<string> GetCommandNames(this Type commandType)
        {
            commandType.Validate(nameof(commandType)).IsNotNull().IsAssignableFrom<Command>();

            var name = Regex.Replace(commandType.Name, $"{nameof(Command)}$", string.Empty, RegexOptions.IgnoreCase);
            var alias = commandType.GetCustomAttribute<AliasAttribute>();

            var @namespace = commandType.GetCustomAttribute<NamespaceAttribute>();

            if (@namespace != null)
            {
                yield return $"{@namespace}.{name}";
                if (alias?.Any() == true)
                {
                    foreach (var x in alias)
                    {
                        yield return $"{@namespace}.{x}";
                    }
                }
            }

            if (@namespace?.Mandatory == true)
            {
                yield break;
            }

            yield return name;
            if (alias?.Any() == true)
            {
                foreach (var x in alias)
                {
                    yield return $"{@namespace}.{x}";
                }
            }
        }

        public static IEnumerable<string> GetPropertyNames(this PropertyInfo property)
        {
            property.Validate(nameof(property)).IsNotNull();

            var customNames = property.GetCustomAttribute<AliasAttribute>();
            if (customNames != null)
            {
                return customNames;
            }

            return new[] { property.Name };
        }

        public static IEnumerable<CommandProperty> GetCommandProperties(this Type commandType)
        {
            commandType.Validate(nameof(commandType)).IsNotNull().IsAssignableFrom<Command>();

            var parameters =
                from property in commandType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where property.GetCustomAttribute<ParameterAttribute>() != null
                select new CommandProperty(property);

            return parameters;
        }

        public static IEnumerable<CommandProperty> GetProperties(this Command command)
        {
            command.Validate(nameof(command)).IsNotNull();

            return command.GetType().GetCommandProperties();
        }

        public static void ValidateCommandPropertyNamesAreUnique(this Type commandType)
        {
            commandType.Validate(nameof(commandType)).IsNotNull().IsAssignableFrom<Command>();

            try
            {
                commandType
                    .GetCommandProperties()
                    .SelectMany(x => x.Names)
                    .ValidateCommandPropertyNamesAreUnique();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Command \"{commandType.FullName}\" has some invalid properties.", ex);
            }
        }

        public static void ValidateCommandPropertyNamesAreUnique(this IEnumerable<string> names)
        {
            var duplicateNames = names
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateNames.Any())
            {
                throw new ArgumentException($"Duplicate names found: [{string.Join(", ", duplicateNames)}].");
            }
        }

        public static string GetDescription(this Type type)
        {
            return type.GetCustomAttribute<DescriptionAttribute>()?.Description;
        }
    }
}
