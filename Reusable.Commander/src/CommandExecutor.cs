using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
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

namespace Reusable.Commander
{
    public interface ICommandExecutor
    {
        //Task ExecuteAsync<TContext>([CanBeNull] string commandLineString, TContext context, CancellationToken cancellationToken = default);

        // You moved ICommandFactory from the the ctor to here because it causes a circular-dependency exception there.
        Task ExecuteAsync<TContext>
        (
            [CanBeNull] string commandLineString,
            [CanBeNull] TContext context,
            [NotNull] ICommandFactory commandFactory,
            CancellationToken cancellationToken = default
        );
    }

    public delegate void ExecuteExceptionCallback(Exception exception);

    [UsedImplicitly]
    public class CommandExecutor : ICommandExecutor
    {
        private const bool Async = true;

        private readonly ILogger _logger;

        private readonly ICommandLineParser _commandLineParser;

        private readonly ExecuteExceptionCallback _executeExceptionCallback;

        public CommandExecutor
        (
            [NotNull] ILogger<CommandExecutor> logger,
            [NotNull] ICommandLineParser commandLineParser,
            [NotNull] ExecuteExceptionCallback executeExceptionCallback
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandLineParser = commandLineParser ?? throw new ArgumentNullException(nameof(commandLineParser));
            _executeExceptionCallback = executeExceptionCallback;
        }

        public async Task ExecuteAsync<TContext>(string commandLineString, TContext context, ICommandFactory commandFactory, CancellationToken cancellationToken)
        {
            if (commandLineString.IsNullOrEmpty())
            {
                throw DynamicException.Create("CommandLineNullOrEmpty", "You need to specify at least one command.");
            }

            var commandLines = _commandLineParser.Parse(commandLineString);
            await ExecuteAsync(commandLines, context, commandFactory, cancellationToken);
        }

        private async Task ExecuteAsync<TContext>(IEnumerable<CommandLineDictionary> commandLines, TContext context, ICommandFactory commandFactory, CancellationToken cancellationToken)
        {
            var executables = GetCommands(commandLines, commandFactory).Select(t => new Executable
                {
                    Command = t.Command,
                    CommandLine = t.CommandLine,
                    Async = new DefaultCommandLine(t.CommandLine).Async
                })
                .ToLookup(e => e.Async);

            _logger.Log(Abstraction.Layer.Service().Counter(new
            {
                CommandCount = executables.Count,
                SequentialCommandCount = executables[!Async].Count(),
                AsyncCommandCount = executables[Async].Count()
            }));

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Execute sequential commands first.
                foreach (var executable in executables[!Async])
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

                foreach (var executable in executables[Async])
                {
                    actionBlock.Post(executable);
                }

                actionBlock.Complete();
                await actionBlock.Completion;
            }
        }

        private async Task ExecuteAsync<TContext>(Executable executable, TContext context, CancellationTokenSource cancellationTokenSource)
        {
            using (_logger.BeginScope().WithCorrelationHandle("Command").AttachElapsed())
            {
                _logger.Log(Abstraction.Layer.Service().Meta(new { CommandName = executable.Command.Id.Default.ToString() }));
                try
                {
                    await executable.Command.ExecuteAsync(executable.CommandLine, context, cancellationTokenSource.Token);
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Completed());
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Canceled(), "Cancelled by user.");
                }
                catch (Exception taskEx)
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Faulted(), taskEx);

                    if (!executable.Async)
                    {
                        cancellationTokenSource.Cancel();
                    }

                    _executeExceptionCallback(taskEx);
                }
            }
        }

        #region Helpers

        private IEnumerable<(ICommand Command, CommandLineDictionary CommandLine)> GetCommands(IEnumerable<CommandLineDictionary> commandLines, ICommandFactory commandFactory)
        {
            foreach (var (commandLine, i) in commandLines.Select((x, i) => (x, i)))
            {
                var commandNameArgument = commandLine[Identifier.Command];
                var commandName = new Identifier((commandNameArgument.Single(), NameOption.CommandLine));
                yield return (commandFactory.CreateCommand(commandName), commandLine);
            }
        }

        #endregion
    }

    internal class Executable
    {
        public ICommand Command { get; set; }

        public CommandLineDictionary CommandLine { get; set; }

        public bool Async { get; set; }
    }
}