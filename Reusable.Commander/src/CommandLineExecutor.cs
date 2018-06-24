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
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface ICommandLineExecutor
    {
        Task<IImmutableList<SoftKeySet>> ExecuteAsync([CanBeNull] string commandLineString, CancellationToken cancellationToken);
    }

    [UsedImplicitly]
    public class CommandLineExecutor : ICommandLineExecutor
    {
        private static readonly IImmutableList<SoftKeySet> NoCommandsExecuted = ImmutableList<SoftKeySet>.Empty;

        private readonly ICommandLineParser _commandLineParser;
        private readonly ICommandFactory _commandFactory;
        private readonly ILogger _logger;

        public CommandLineExecutor([NotNull] ILoggerFactory loggerFactory, [NotNull] ICommandLineParser commandLineParser, [NotNull] ICommandFactory commandFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(CommandLineExecutor)) ?? throw new ArgumentNullException(nameof(loggerFactory));
            _commandLineParser = commandLineParser ?? throw new ArgumentNullException(nameof(commandLineParser));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        }

        //public static ICommandLineExecutor Create([NotNull] ILoggerFactory loggerFactory, [NotNull] ICommandRegistrationContainer commands)
        //{
        //    if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        //    if (commands == null) throw new ArgumentNullException(nameof(commands));

        //    var builder = new ContainerBuilder();
            
        //    builder
        //        .RegisterInstance(loggerFactory)
        //        .As<ILoggerFactory>();
            
        //    builder
        //        .RegisterModule(new CommanderModule(commands));
            
        //    using (var container = builder.Build())
        //    using (var scope = container.BeginLifetimeScope())
        //    {
        //        return scope.Resolve<ICommandLineExecutor>();
        //    }
        //}

        public async Task<IImmutableList<SoftKeySet>> ExecuteAsync(string commandLineString, CancellationToken cancellationToken)
        {
            if (commandLineString.IsNullOrEmpty())
            {
                return NoCommandsExecuted;
            }

            var commandLines = _commandLineParser.Parse(commandLineString);

            var executables =
                (from commandLine in commandLines
                 let commandName = commandLine.CommandName()
                 let command = _commandFactory.CreateCommand(commandName, commandLine)
                 select (commandName, command)).ToList();

            var mismatchCommands = executables.Where(exe => exe.command is null).ToList();
            if (mismatchCommands.Any())
            {
                var notFoundCommandNames = mismatchCommands.Select(exe => exe.commandName.FirstLongest().ToString());
                throw DynamicException.Factory.CreateDynamicException(
                    $"CommandNotFound{nameof(Exception)}", 
                    $"Could not find one or more commands: {notFoundCommandNames.Join(", ").EncloseWith("[]")}.", 
                    null);
            }            

            var executedCommands = new List<SoftKeySet>();

            foreach (var (softKeySet, consoleCommand) in executables)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await consoleCommand.ExecuteAsync(cancellationToken);
                executedCommands.Add(softKeySet);
            }

            return executedCommands.ToImmutableList();            
        }
    }
}