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
using System.Threading.Tasks.Dataflow;
using Autofac;
using Autofac.Core;
using Autofac.Features.AttributeFilters;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Commander.Commands;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Reflection;

namespace Reusable.Commander
{
    using static CommandExecutionType;

    public interface ICommandLineExecutor
    {
        Task ExecuteAsync<TContext>([CanBeNull] string commandLineString, TContext context, CancellationToken cancellationToken = default);

        //Task ExecuteAsync<TContext>([NotNull, ItemNotNull] IEnumerable<ICommandLine> commandLines, TContext context, CancellationToken cancellationToken = default);

        //Task ExecuteAsync<TBag>([NotNull] Identifier identifier, [CanBeNull] TBag parameter = default, CancellationToken cancellationToken = default) where TBag : ICommandBag, new();
    }

    public delegate void ExecuteExceptionCallback(Exception exception);

    [UsedImplicitly]
    public class CommandLineExecutor : ICommandLineExecutor
    {
        private readonly ILogger _logger;
        private readonly ICommandLineParser _commandLineParser;
        private readonly ICommandLineMapper _mapper;
        private readonly IIndex<Identifier, IConsoleCommand> _commands;
        private readonly ExecuteExceptionCallback _executeExceptionCallback;

        public CommandLineExecutor
        (
            [NotNull] ILogger<CommandLineExecutor> logger,
            [NotNull] ICommandLineParser commandLineParser,
            [NotNull] ICommandLineMapper mapper,
            [NotNull] IIndex<Identifier, IConsoleCommand> commands,
            [NotNull] ExecuteExceptionCallback executeExceptionCallback
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandLineParser = commandLineParser ?? throw new ArgumentNullException(nameof(commandLineParser));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _executeExceptionCallback = executeExceptionCallback;
        }

        private async Task ExecuteAsync<TContext>(IEnumerable<ICommandLine> commandLines, TContext context, CancellationToken cancellationToken)
        {
            var executables =
                GetCommands(commandLines)
                    .Select(t => new Executable
                    {
                        Command = t.Command,
                        CommandLine = t.CommandLine,
                        Bag = _mapper.Map<SimpleBag>(t.CommandLine)
                    })
                    .ToLookup(x => x.Bag.ExecutionType());

            _logger.Log(Abstraction.Layer.Infrastructure().Counter(new
            {
                CommandCount = executables.Count,
                SequentialCommandCount = executables[Sequential].Count(),
                AsyncCommandCount = executables[Asynchronous].Count()
            }));


            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Execute sequential commands first.
                foreach (var executable in executables[Sequential])
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await ExecuteAsync(executable, context, cts);
                }

                // Now execute async commands.
                var actionBlock = new ActionBlock<Executable>
                (
                    async executable => await ExecuteAsync(executable, context, cts),
                    new ExecutionDataflowBlockOptions
                    {
                        CancellationToken = cts.Token,
                        MaxDegreeOfParallelism = Environment.ProcessorCount
                    }
                );

                foreach (var executable in executables[Asynchronous])
                {
                    actionBlock.Post(executable);
                }

                actionBlock.Complete();
                await actionBlock.Completion;
            }
        }

        public async Task ExecuteAsync<TContext>(string commandLineString, TContext context, CancellationToken cancellationToken)
        {
            if (commandLineString.IsNullOrEmpty())
            {
                throw DynamicException.Create($"CommandLineStringNullOrEmpty", $"You need to specify at least one command.");
            }

            var commandLines = _commandLineParser.Parse(commandLineString);
            await ExecuteAsync(commandLines, context, cancellationToken);
        }

//        public async Task ExecuteAsync<TBag>(Identifier identifier, TBag parameter, CancellationToken cancellationToken = default) where TBag : ICommandBag, new()
//        {
//            if (identifier == null) throw new ArgumentNullException(nameof(identifier));
//
//            await GetCommand(identifier).ExecuteAsync(parameter, default, cancellationToken);
//        }

        private async Task ExecuteAsync<TContext>(Executable executable, TContext context, CancellationTokenSource cancellationTokenSource)
        {
            using (_logger.BeginScope().WithCorrelationHandle("Command").AttachElapsed())
            {
                _logger.Log(Abstraction.Layer.Infrastructure().Meta(new { CommandName = executable.Command.Id.Default.ToString() }));
                try
                {
                    await executable.Command.ExecuteAsync(executable.CommandLine, context, cancellationTokenSource.Token);
                    _logger.Log(Abstraction.Layer.Infrastructure().Routine(nameof(IConsoleCommand.ExecuteAsync)).Completed());
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(Abstraction.Layer.Infrastructure().Routine(nameof(IConsoleCommand.ExecuteAsync)).Canceled(), "Cancelled by user.");
                }
                catch (Exception taskEx)
                {
                    _logger.Log(Abstraction.Layer.Infrastructure().Routine(nameof(IConsoleCommand.ExecuteAsync)).Faulted(), taskEx);

                    if (!executable.Bag.Async)
                    {
                        cancellationTokenSource.Cancel();
                    }

                    _executeExceptionCallback(taskEx);
                }
            }
        }

        #region Helpers

        private IEnumerable<(IConsoleCommand Command, ICommandLine CommandLine)> GetCommands(IEnumerable<ICommandLine> commandLines)
        {
            return commandLines.Select((commandLine, i) =>
            {
                try
                {
                    var commandName = commandLine.CommandId();
                    return (GetCommand(commandName), commandLine);
                }
                catch (DynamicException inner)
                {
                    throw DynamicException.Create
                    (
                        $"InvalidCommandLine",
                        $"Command line at {i} is invalid. See the inner-exception for details.",
                        inner
                    );
                }
            });
        }

        [NotNull]
        private IConsoleCommand GetCommand(Identifier id)
        {
            return
                _commands.TryGetValue(id, out var command)
                    ? command
                    : throw DynamicException.Create
                    (
                        $"CommandNotFound",
                        $"Could not find command '{id.Default.ToString()}'."
                    );
        }

        #endregion

        private class Executable
        {
            public IConsoleCommand Command { get; set; }
            public ICommandLine CommandLine { get; set; }
            public ICommandParameter Bag { get; set; }
        }
    }
}