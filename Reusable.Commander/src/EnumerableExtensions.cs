using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Reusable.Reflection;

namespace Reusable.Commander
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<(IConsoleCommand Command, ICommandLine CommandLine)> Find(this IIndex<Identifier, IConsoleCommand> commands, IEnumerable<ICommandLine> commandLines)
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
        private static IConsoleCommand Find(this IIndex<Identifier, IConsoleCommand> commands, Identifier commandId)
        {
            return
                commands.TryGetValue(commandId, out var command)
                    ? command
                    : throw DynamicException.Factory.CreateDynamicException(
                        $"CommandNotFound{nameof(Exception)}",
                        $"Could not find command '{commandId.Default.ToString()}'."
                    );
        }
    }
}