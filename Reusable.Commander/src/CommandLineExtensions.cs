using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.Commander
{
    public static class CommandLineExtensions
    {
        [NotNull]
        public static CommandParameter AnonymousParameter([NotNull] this ICommandLine commandLine)
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            return commandLine[Identifier.Empty] ?? new CommandParameter(Identifier.Empty);
        }

        [NotNull]
        public static Identifier CommandId([NotNull] this ICommandLine commandLine)
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            // Command-name is the first anonymous argument.
            return
                commandLine
                    .AnonymousParameter()
                    .FirstOrDefault()
                ?? throw DynamicException.Factory.CreateDynamicException(
                    $"CommandNameNotFound{nameof(Exception)}",
                    $"Command line '{commandLine}' does not contain a command name."
                );
        }
    }
}