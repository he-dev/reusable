using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using Reusable.Reflection;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander
{
    public interface ICommandLineExecutor
    {
        Task<IReadOnlyList<SoftKeySet>> ExecuteAsync([CanBeNull] string commandLineString, CancellationToken cancellationToken);
    }

    [UsedImplicitly]
    public class CommandLineExecutor : ICommandLineExecutor
    {
        private static readonly IReadOnlyList<SoftKeySet> NoCommandsExecuted = new List<SoftKeySet>();

        private readonly ICommandLineParser _commandLineParser;
        private readonly ICommandFactory _commandFactory;
        private readonly ILogger _logger;

        public CommandLineExecutor([NotNull] ILoggerFactory loggerFactory, [NotNull] ICommandLineParser commandLineParser, [NotNull] ICommandFactory commandFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(CommandLineExecutor)) ?? throw new ArgumentNullException(nameof(loggerFactory));
            _commandLineParser = commandLineParser ?? throw new ArgumentNullException(nameof(commandLineParser));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        }

        public static ICommandLineExecutor Create([NotNull] ILoggerFactory loggerFactory, [NotNull] ICommandRegistrationContainer commands)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (commands == null) throw new ArgumentNullException(nameof(commands));

            var builder = new ContainerBuilder();
            
            builder
                .RegisterInstance(loggerFactory)
                .As<ILoggerFactory>();
            
            builder
                .RegisterModule(new CommanderModule(commands));
            
            using (var container = builder.Build())
            using (var scope = container.BeginLifetimeScope())
            {
                return scope.Resolve<ICommandLineExecutor>();
            }
        }

        public async Task<IReadOnlyList<SoftKeySet>> ExecuteAsync(string commandLineString, CancellationToken cancellationToken)
        {
            if (commandLineString.IsNullOrEmpty())
            {
                return NoCommandsExecuted;
            }

            var commandLines = _commandLineParser.Parse(commandLineString).ToList();

            var executables =
                (from commandLine in commandLines
                 let commandName = commandLine.CommandName()
                 let command = _commandFactory.CreateCommand(commandName, commandLine)
                 select (commandName, command)).ToList();

            var notFoundCommands = executables.Where(exe => exe.command is null).ToList();
            if (notFoundCommands.Any())
            {
                // From each command get only a single name but take the longest one as this is most likely the full-name.
                var notFoundCommandNames = notFoundCommands.Select(exe => exe.commandName.OrderByDescending(name => name.Length).First().ToString());
                throw DynamicException.Factory.CreateDynamicException(
                    $"CommandNotFound{nameof(Exception)}", 
                    $"Could not find one or more commands: {notFoundCommandNames.Join(", ").EncloseWith("[]")}.", 
                    null);
            }            

            var executedCommands = new List<SoftKeySet>();

            foreach (var executable in executables)
            {
                await executable.command.ExecuteAsync(cancellationToken);
                executedCommands.Add(executable.commandName);
            }

            return executedCommands;            
        }
    }

    // Allows to specify the culture for a parameter
    public class CultureAttirbute : Attribute
    {
    }
}