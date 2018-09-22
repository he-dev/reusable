using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Commander.Commands;
using Reusable.Converters;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface ICommandLineExecutor
    {
        Task<IImmutableList<SoftKeySet>> ExecuteAsync([NotNull, ItemNotNull] IList<ICommandLine> commandLines, CancellationToken cancellationToken = default);

        Task<IImmutableList<SoftKeySet>> ExecuteAsync([CanBeNull] string commandLineString, CancellationToken cancellationToken = default);
    }

    [UsedImplicitly]
    public class CommandLineExecutor : ICommandLineExecutor
    {
        private static readonly IImmutableList<SoftKeySet> NoCommandsExecuted = ImmutableList<SoftKeySet>.Empty;

        private readonly ICommandLineParser _commandLineParser;
        private readonly IEnumerable<IConsoleCommand> _commands;
        private readonly ILogger _logger;

        public CommandLineExecutor(
            [NotNull] ILogger<CommandLineExecutor> logger,
            [NotNull] ICommandLineParser commandLineParser,
            [NotNull] IEnumerable<IConsoleCommand> commands)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandLineParser = commandLineParser ?? throw new ArgumentNullException(nameof(commandLineParser));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public async Task<IImmutableList<SoftKeySet>> ExecuteAsync(IList<ICommandLine> commandLines, CancellationToken cancellationToken)
        {
            if (commandLines.Any())
            {
                var executables =
                (
                    from commandLine in commandLines
                    let commandName = commandLine.CommandName()
                    let command = _commands.SingleOrDefault(x => x.Name == commandName)
                    select (commandLine, commandName, command)
                ).ToList();

                var notFoundCommands = executables.Where(exe => exe.command is null).ToList();
                if (notFoundCommands.Any())
                {
                    var notFoundCommandNames = notFoundCommands.Select(exe => exe.commandName.FirstLongest().ToString());
                    throw DynamicException.Factory.CreateDynamicException(
                        $"CommandNotFound{nameof(Exception)}",
                        $"Could not find one or more commands: {notFoundCommandNames.Join(", ").EncloseWith("[]")}.",
                        null);
                }

                var executedCommands = new List<SoftKeySet>();

                // In command-line mode execute command sequentially.
                foreach (var (commandLine, commandName, command) in executables)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    await command.ExecuteAsync(commandLine, cancellationToken);
                    executedCommands.Add(commandName);
                }

                return executedCommands.ToImmutableList();
            }
            else
            {
                var executables =
                (
                    from command in _commands
                    select (commandLine: default(CommandLine), command.Name, command)
                ).ToList();

                var tasks = executables.Select(async executable =>
                {
                    var loggerScope = _logger.BeginScope().WithCorrelationContext(new { Command = executable.command.Name.ToString() }).AttachElapsed();
                    try
                    {
                        await executable.command.ExecuteAsync(executable.commandLine, cancellationToken);
                        _logger.Log(Abstraction.Layer.Infrastructure().Routine(nameof(IConsoleCommand.ExecuteAsync)).Completed());
                    }
                    catch (Exception taskEx)
                    {
                        _logger.Log(Abstraction.Layer.Infrastructure().Routine(nameof(IConsoleCommand.ExecuteAsync)).Faulted(), taskEx);
                    }
                    finally
                    {
                        loggerScope.Dispose();
                    }
                }).ToArray();

                await Task.WhenAll(tasks);                

                return executables.Select(x => x.Name).ToImmutableList();
            }
        }

        public async Task<IImmutableList<SoftKeySet>> ExecuteAsync(string commandLineString, CancellationToken cancellationToken)
        {
            return
                commandLineString.IsNullOrEmpty()
                    ? NoCommandsExecuted
                    : await ExecuteAsync(_commandLineParser.Parse(commandLineString).ToList(), cancellationToken);
        }
    }
}