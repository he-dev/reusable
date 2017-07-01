using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.Extensions;

namespace Reusable.CommandLine.Services
{
    internal static class CommandNameFactory
    {
        [NotNull]
        public static IImmutableNameSet From([NotNull] Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (!typeof(ICommand).IsAssignableFrom(type))
            {
                throw new ArgumentException(paramName: nameof(type), message: $"'{nameof(type)}' needs to be derived from '{nameof(ICommand)}'");
            }

            return ImmutableNameSet.Create(GetCommandNames(type));
        }

        private static IEnumerable<string> GetCommandNames(MemberInfo type)
        {
            var commandNames = type.GetCustomAttribute<CommandNamesAttribute>();

            // Without an attribute there can only be the default name.
            if (commandNames == null)
            {
                // Without an attribute there is only one name, the default one. No short name.
                foreach (var name in GetDefaultNames(type, false))
                {
                    yield return name;
                }
            }
            else
            {
                // Even though the attribute is used it might still not customize the name so use the default one if no custom ones are defined.
                var names = commandNames.Any() ? commandNames : GetDefaultNames(type, commandNames.AllowShortName);
                foreach (var name in names)
                {
                    // If the namespace is optional then use default names too.
                    if (!commandNames.NamespaceRequired)
                    {
                        yield return name;
                    }

                    // If there is a namespace then use it as a prefix.
                    if (commandNames.Namespace.IsNotNullOrEmpty())
                    {
                        yield return $"{commandNames.Namespace}.{name}";
                    }
                }
            }
        }

        private static IEnumerable<string> GetDefaultNames(MemberInfo type, bool allowShortName)
        {
            var defaultName = Regex.Replace(type.Name, "C(omman|m)d$", string.Empty, RegexOptions.IgnoreCase);
            yield return defaultName;
            if (allowShortName)
            {
                yield return CreateShortName(defaultName);
            }
        }

        // Creates a short name from the capital letters.
        private static string CreateShortName(string fullName)
        {
            return Regex
                .Matches(fullName, "[A-Z]")
                .Cast<Match>()
                .Select(m => m.Groups[0].Value)
                .Aggregate(new StringBuilder(), (current, next) => current.Append(next))
                .ToString();
        }
    }
}
