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
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!typeof(ICommand).IsAssignableFrom(type))
            {
                throw new ArgumentException(paramName: nameof(type), message: $"'{nameof(type)}' needs to be derived from '{nameof(ICommand)}'");
            }

            return ImmutableNameSet.Create(GetCommandNames(type));            
        }

        private static IEnumerable<string> GetCommandNames(MemberInfo type)
        {
            var commandNames = type.GetCustomAttribute<CommandNamesAttribute>();
            if (commandNames == null)
            {
                foreach (var name in GetDefaultNames(false))
                {
                    yield return name;
                }
            }
            else
            {
                var names = commandNames.Any() ? commandNames : GetDefaultNames(commandNames.AllowShortName);
                if (commandNames.Namespace.IsNotNullOrEmpty())
                {
                    foreach (var name in names)
                    {
                        if (!commandNames.NamespaceRequired)
                        {
                            yield return name;
                        }
                        yield return $"{commandNames.Namespace}.{name}";
                    }
                }
            }

            IEnumerable<string> GetDefaultNames(bool allowShortName)
            {
                var defaultName = Regex.Replace(type.Name, "C(omman|m)d$", string.Empty, RegexOptions.IgnoreCase);
                yield return defaultName;
                if (allowShortName)
                {
                    yield return CreateShortName(defaultName);
                }
            }
        }

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
