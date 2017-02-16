using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reusable.Shelly
{
    internal class CommandReflection
    {
        public static IEnumerable<CommandProperty> GetCommandProperties(Type commandType)
        {
            return
                from property in commandType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where property.GetCustomAttribute<ParameterAttribute>() != null
                select new CommandProperty(property);
        }

        public static IList<string> GetCommandNames(Type commandType)
        {
            var names = new List<string>();

            var commandName =
                commandType.GetCustomAttribute<CommandNameAttribute>() ??
                Regex.Replace(commandType.Name, $"Command$", string.Empty, RegexOptions.IgnoreCase);

            var shotcutAttribute = commandType.GetCustomAttribute<ShortcutAttribute>() ?? Enumerable.Empty<string>();
            var namespaceAttribute = commandType.GetCustomAttribute<NamespaceAttribute>();

            if (namespaceAttribute != null)
            {
                names.Add($"{namespaceAttribute}.{commandName}");
                names.AddRange((shotcutAttribute).Select(x => $"{namespaceAttribute}.{x}"));

                // Jump over command name and shortcuts.
                if (namespaceAttribute.Mandatory) goto sort;
            }

            names.Add(commandName);
            names.AddRange(shotcutAttribute);

            sort:

            return names.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static IEnumerable<string> GetCommandName(ICommand command) => GetCommandNames(command.GetType());
    }
}
