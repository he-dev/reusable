using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
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
        // You moved ICommandFactory from the the ctor to here because it causes a circular-dependency exception there.
        Task ExecuteAsync<TContext>(string commandLineString, TContext context = default, CancellationToken cancellationToken = default);
    }

    [UsedImplicitly]
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ILogger _logger;
        private readonly ICommandLineParser _commandLineParser;
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandParameterBinder _commandParameterBinder;

        public CommandExecutor
        (
            ILogger<CommandExecutor> logger,
            ICommandLineParser commandLineParser,
            ICommandFactory commandFactory,
            ICommandParameterBinder commandParameterBinder
        )
        {
            _logger = logger;
            _commandLineParser = commandLineParser;
            _commandFactory = commandFactory;
            _commandParameterBinder = commandParameterBinder;
        }

        public async Task ExecuteAsync<TContext>(string commandLineString, TContext context = default, CancellationToken cancellationToken = default)
        {
            if (commandLineString.IsNullOrEmpty())
            {
                throw DynamicException.Create("CommandLineNullOrEmpty", "You need to specify at least one command.");
            }

            var commandLines = _commandLineParser.Parse(commandLineString);

            var executables =
                from t in commandLines.Select((commandLine, index) => (commandLine, index))
                let arg0 = t.commandLine.Where(a => a.Name.Equals(MultiName.Command)).SingleOrThrow(onEmpty: ("CommandNameNotFound", $"Command line {t.index} does not contain command-name."))
                let command = _commandFactory.CreateCommand(arg0.Name.Single())
                select
                (
                    t.index,
                    command,
                    t.commandLine
                );

            var async = executables.ToLookup(e => _commandParameterBinder.Bind<CommandParameter>(e.commandLine).Async);

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

                    var commandParameterType = executable.command.GetType().GetCommandParameterType();
                    var bindMethod = typeof(ICommandParameterBinder).GetMethod(nameof(ICommandParameterBinder.Bind))!.MakeGenericMethod(commandParameterType ?? typeof(object));
                    var parameter = bindMethod.Invoke(_commandParameterBinder, new object[] { context, executable.commandLine });
                    var task = executable.command.ExecuteAsync(parameter, cts.Token);
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
                var actionBlock = new ActionBlock<(int index, ICommand command, List<CommandLineArgument> commandLine)>
                (
                    async executable =>
                    {
                        var commandParameterType = executable.command.GetType().GetCommandParameterType();
                        var bindMethod = typeof(ICommandParameterBinder).GetMethod(nameof(ICommandParameterBinder.Bind))!.MakeGenericMethod(commandParameterType ?? typeof(object));
                        var parameter = bindMethod.Invoke(_commandParameterBinder, new object[] { context, executable.commandLine });
                        var task = executable.command.ExecuteAsync(parameter, cts.Token);
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