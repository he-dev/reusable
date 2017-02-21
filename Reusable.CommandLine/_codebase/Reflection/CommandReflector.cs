using Reusable.Shelly.Collections;
using Reusable.Shelly.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reusable.Shelly
{
    internal class CommandReflector
    {
        public static ImmutableHashSet<string> GetNames(Type commandType)
        {
            var names = new List<string>();

            var commandName = GetCommandNameOrDefault();
            var shotcutAttribute = commandType.GetCustomAttribute<ShortcutsAttribute>() ?? Enumerable.Empty<string>();
            var namespaceAttribute = commandType.GetCustomAttribute<NamespaceAttribute>();

            if (namespaceAttribute != null)
            {
                names.Add($"{namespaceAttribute}.{commandName}");
                names.AddRange((shotcutAttribute).Select(x => $"{namespaceAttribute}.{x}"));

                if (namespaceAttribute.Mandatory) goto skipNamesWithoutNamespace;
            }

            names.Add(commandName);
            names.AddRange(shotcutAttribute);

            skipNamesWithoutNamespace:

            return ImmutableNameSet.Create(names.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray());

            string GetCommandNameOrDefault() =>
                commandType.GetCustomAttribute<CommandNameAttribute>() ??
                Regex.Replace(commandType.Name, $"Command$", string.Empty, RegexOptions.IgnoreCase);
        }

        public static ImmutableHashSet<string> GetNames(ICommand command) => GetNames(command.GetType());
    }
}
