using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Reflection;

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

        [NotNull]
        public static SoftKeySet CommandName([NotNull] this ICommandLine commandLine)
        {
            // Command-name is the first anonymous argument.
            return
                commandLine
                    .AnonymousValues()
                    .FirstOrDefault()
                    ?? throw DynamicException.Factory.CreateDynamicException(
                            $"CommandNameNotFound{nameof(Exception)}",
                            $"Command line '{commandLine}' does not contain a command name."
                    );
        }
    }
}