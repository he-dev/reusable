using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Commander
{
    public static class CommandLineExtensions
    {
        [NotNull, ItemNotNull]
        public static IEnumerable<string> AnonymousValues([NotNull] this ICommandLine commandLine) => commandLine[CommandArgumentKeys.Anonymous];

        [NotNull, ItemNotNull]
        public static IEnumerable<string> ArgumentValues(this ICommandLine commandLine, int? position, SoftKeySet name)
        {
            return
                position.HasValue 
                    ? commandLine.AnonymousValues().Skip(position.Value).Take(1) 
                    : commandLine[name];
        }

        [CanBeNull]
        public static SoftKeySet CommandName([NotNull] this ICommandLine commandLine)
        {
            // Command-name is the first anonymous argument.
            var commandName = commandLine.AnonymousValues().FirstOrDefault();
            return string.IsNullOrEmpty(commandName) ? default : commandName;
        }
    }
}