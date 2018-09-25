using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Reflection;

namespace Reusable.Commander
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<(IConsoleCommand Command, ICommandLine CommandLine)> Find(this IEnumerable<IConsoleCommand> commands, IEnumerable<ICommandLine> commandLines)
        {
            return commandLines.Select(
                (commandLine, i) =>
                {
                    try
                    {
                        var commandName = commandLine.CommandName();
                        return (commands.Find(commandName), commandLine);
                    }
                    catch (DynamicException ex)
                    {
                        throw DynamicException.Factory.CreateDynamicException(
                            $"InvalidCommandLine{nameof(Exception)}",
                            $"Command line at {i} is invalid. See the inner-exception for details.",
                            ex
                        );
                    }
                }
            );
        }

        [NotNull]
        public static IConsoleCommand Find(this IEnumerable<IConsoleCommand> commands, SoftKeySet name)
        {
            return
                commands.SingleOrDefault(x => x.Name == name)
                ?? throw DynamicException.Factory.CreateDynamicException(
                    $"CommandNotFound{nameof(Exception)}",
                    $"Could not find command '{name.FirstLongest().ToString()}'."
                );
        }
    }
}