using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Commander
{
    public static class CommandLineExtensions
    {
        [NotNull]
        public static IEnumerable<string> Anonymous([NotNull] this ICommandLine arguments) => arguments[SoftString.Empty];

        public static IEnumerable<string> Values(this ICommandLine commandLine, CommandParameter parameter)
        {
            if (parameter.Position.HasValue)
            {
                return commandLine.Anonymous().Skip(parameter.Position.Value).Take(1);
            }

            if (commandLine.Contains(parameter.Name))
            {
                return commandLine[parameter.Name];
            }

            return CommandArgument.Undefined;
        }

        [CanBeNull]
        public static SoftKeySet CommandName([NotNull] this ICommandLine arguments)
        {
            // Command-name is the first value.
            var commandName = arguments[CommandArgument.CommandNameKey].First();
            return string.IsNullOrEmpty(commandName) ? default : commandName;
        }
    }
}