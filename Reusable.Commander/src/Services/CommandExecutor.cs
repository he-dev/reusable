using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Commander.Services
{
    public interface ICommandExecutor
    {
        Task ExecuteAsync<TContext>([CanBeNull] string commandLineString, TContext context, CancellationToken cancellationToken = default);

        //Task ExecuteAsync<TContext>([NotNull, ItemNotNull] IEnumerable<ICommandLine> commandLines, TContext context, CancellationToken cancellationToken = default);

        //Task ExecuteAsync<TBag>([NotNull] Identifier identifier, [CanBeNull] TBag parameter = default, CancellationToken cancellationToken = default) where TBag : ICommandBag, new();
    }

    public delegate void ExecuteExceptionCallback(Exception exception);

    [UsedImplicitly]
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ILogger _logger;
        private readonly ICommandLineParser _commandLineParser;
        private readonly IIndex<Identifier, IConsoleCommand> _commands;
        private readonly ExecuteExceptionCallback _executeExceptionCallback;

        public CommandExecutor
        (
            [NotNull] ILogger<CommandExecutor> logger,
            [NotNull] ICommandLineParser commandLineParser,
            [NotNull] IIndex<Identifier, IConsoleCommand> commands,
            [NotNull] ExecuteExceptionCallback executeExceptionCallback
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandLineParser = commandLineParser ?? throw new ArgumentNullException(nameof(commandLineParser));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _executeExceptionCallback = executeExceptionCallback;
        }

        private async Task ExecuteAsync<TContext>(IEnumerable<ICommandLine> commandLines, TContext context, CancellationToken cancellationToken)
        {
            var executables =
                GetCommands(commandLines)
                    .Select(t =>
                    {
                        var commandLineReader = new CommandLineReader<ICommandParameter>(t.CommandLine);
                        return new Executable
                        {
                            Command = t.Command,
                            CommandLine = t.CommandLine,
                            Async = commandLineReader.GetItem(x => x.Async)
                        };
                    })
                    .ToLookup(e => e.ExecutionType());

            _logger.Log(Abstraction.Layer.Service().Counter(new
            {
                CommandCount = executables.Count,
                SequentialCommandCount = executables[CommandExecutionType.Sequential].Count(),
                AsyncCommandCount = executables[CommandExecutionType.Asynchronous].Count()
            }));


            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Execute sequential commands first.
                foreach (var executable in executables[CommandExecutionType.Sequential])
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

                foreach (var executable in executables[CommandExecutionType.Asynchronous])
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
                throw DynamicException.Create("CommandLineNullOrEmpty", "You need to specify at least one command.");
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
                _logger.Log(Abstraction.Layer.Service().Meta(new { CommandName = executable.Command.Id.Default.ToString() }));
                try
                {
                    await executable.Command.ExecuteAsync(executable.CommandLine, context, cancellationTokenSource.Token);
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(IConsoleCommand.ExecuteAsync)).Completed());
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(IConsoleCommand.ExecuteAsync)).Canceled(), "Cancelled by user.");
                }
                catch (Exception taskEx)
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(IConsoleCommand.ExecuteAsync)).Faulted(), taskEx);

                    if (!executable.Async)
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
                        $"CommandLine",
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
    }

    internal class Executable
    {
        public ICommandLine CommandLine { get; set; }

        public IConsoleCommand Command { get; set; }

        public bool Async { get; set; }
    }
}