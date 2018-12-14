using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Exceptionizer;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public static class CommandLineExtensions
    {
        [NotNull, ItemNotNull]
        public static IEnumerable<string> AnonymousValues([NotNull] this ICommandLine commandLine)
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            return commandLine[Identifier.Empty];
        }

        //[NotNull, ItemNotNull]
        //public static IEnumerable<string> ArgumentValues([NotNull] this ICommandLine commandLine, int? position, Identifier id)
        //{
        //    if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

        //    return
        //        position.HasValue
        //            ? commandLine.AnonymousValues().Skip(position.Value).Take(1)
        //            : commandLine[id];
        //}

        public static bool TryGetArgumentValues([NotNull] this ICommandLine commandLine, Identifier id, int? position, [NotNull, ItemNotNull] out IList<string> values)
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            if (commandLine.Contains(id))
            {
                values = commandLine[id].ToList();
                return true;
            }

            if (position.HasValue && position.Value <= commandLine.AnonymousValues().Count() - 1)
            {
                values = new[] { commandLine.AnonymousValues().ElementAtOrDefault(position.Value) };
                return true;
            }

            values = default;
            return false;
        }

        [NotNull]
        public static Identifier CommandId([NotNull] this ICommandLine commandLine)
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

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