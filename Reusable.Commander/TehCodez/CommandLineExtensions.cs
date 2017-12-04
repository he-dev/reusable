using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander
{
    public static class CommandLineExtensions
    {
        [NotNull]
        public static IEnumerable<string> Anonymous([NotNull] this ICommandLine arguments) => arguments[ImmutableKeySet<SoftString>.Empty];

        [CanBeNull]
        public static ImmutableKeySet<SoftString> CommandName([NotNull] this ICommandLine arguments)
        {
            // Command-name is the first value.
            var commandName = arguments[CommandArgument.CommandNameKey].First();
            return string.IsNullOrEmpty(commandName) ? null : ImmutableKeySet<SoftString>.Create(commandName);
        }
    }
}