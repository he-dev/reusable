using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

    [UsedImplicitly]
    public class CommandExecutor : ICommandExecutor
    {
        //private const bool Async = true;

        private readonly ILogger _logger;

        private readonly ICommandLineParser _commandLineParser;


        public CommandExecutor
        (
            [NotNull] ILogger<CommandExecutor> logger,
            [NotNull] ICommandLineParser commandLineParser
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandLineParser = commandLineParser ?? throw new ArgumentNullException(nameof(commandLineParser));
        }

        public async Task ExecuteAsync<TContext>(string commandLineString, TContext context, ICommandFactory commandFactory, CancellationToken cancellationToken)
        {
            if (commandLineString.IsNullOrEmpty())
            {
                throw DynamicException.Create("CommandLineNullOrEmpty", "You need to specify at least one command.");
            }

            var commandLines = _commandLineParser.Parse(commandLineString);

            var executables =
                from t in commandLines.Select((commandLine, index) => (commandLine, index))
                let commandNameArgument = t.commandLine[NameSet.Command]
                let commandName = new NameSet((commandNameArgument.Single(), Name.Options.CommandLine))
                let command = commandFactory.CreateCommand(commandName)
                select
                (
                    t.index,
                    command,
                    t.commandLine
                );

            var async = executables.ToLookup(e => new CommandLineBase(e.commandLine).Async);

            _logger.Log(Abstraction.Layer.Service().Counter(new { CommandCount = async.Count, SequentialCommandCount = async[false].Count(), AsyncCommandCount = async[true].Count() }));

            var exceptions = new ConcurrentBag<Exception>();

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Execute sequential commands first.
                foreach (var executable in async[false])
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var task = executable.command.ExecuteAsync(executable.commandLine, context, cts.Token);
                    var continuation = task.ContinueWith(t =>
                    {
                        exceptions.Add(t.Exception);
                        cts.Cancel();
                    }, TaskContinuationOptions.OnlyOnFaulted);

                    try
                    {
                        await continuation;
                    }
                    catch (TaskCanceledException)
                    {
                        /* ignore this exception */
                    }
                }
            }

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Now execute async commands.
                var actionBlock = new ActionBlock<(int index, ICommand command, CommandLineDictionary commandLine)>
                (
                    async executable =>
                    {
                        var task = executable.command.ExecuteAsync(executable.commandLine, context, cts.Token);
                        var continuation = task.ContinueWith(t => exceptions.Add(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
                        try
                        {
                            await continuation;
                        }
                        catch (TaskCanceledException)
                        {
                            /* ignore this exception */
                        }
                    },
                    new ExecutionDataflowBlockOptions
                    {
                        CancellationToken = cts.Token,
                        MaxDegreeOfParallelism = Environment.ProcessorCount
                    }
                );

                foreach (var executable in async[true])
                {
                    actionBlock.Post(executable);
                }

                actionBlock.Complete();
                await actionBlock.Completion;
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}