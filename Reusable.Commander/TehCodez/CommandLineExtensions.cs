using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.CommandLine;

namespace Reusable.Commander
{
    public static class CommandLineExtensions
    {
        [NotNull]
        public static IEnumerable<string> Anonymous([NotNull] this ICommandLine arguments) => arguments[SoftKeySet.Empty];

        [CanBeNull]
        public static SoftKeySet CommandName([NotNull] this ICommandLine arguments)
        {
            // Command-name is the first value.
            var commandName = arguments[CommandArgument.CommandNameKey].First();
            return string.IsNullOrEmpty(commandName) ? null : SoftKeySet.Create(commandName);
        }
    }
}