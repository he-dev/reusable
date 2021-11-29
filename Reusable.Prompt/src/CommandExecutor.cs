using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Autofac;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;

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
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ICommandParameterBinder _commandParameterBinder;
        private readonly IEnumerable<CommandInfo> _commands;

        public CommandExecutor
        (
            ILogger<CommandExecutor> logger,
            ICommandLineParser commandLineParser,
            ILifetimeScope lifetimeScope,
            ICommandParameterBinder commandParameterBinder,
            IEnumerable<CommandInfo> commands
        )
        {
            _logger = logger;
            _commandLineParser = commandLineParser;
            _lifetimeScope = lifetimeScope;
            _commandParameterBinder = commandParameterBinder;
            _commands = commands;
        }

        public async Task ExecuteAsync<TContext>(string commandLineString, TContext context = default, CancellationToken cancellationToken = default)
        {
            if (commandLineString.IsNullOrEmpty())
            {
                throw DynamicException.Create("CommandLineNullOrEmpty", "You need to specify at least one command.");
            }

            var commandLines = _commandLineParser.Parse(commandLineString);

            var executables =
                from t in commandLines.Select((args, index) => (args, index))
                let arg0 = t.args.Where(a => a.Name.Equals(ArgumentName.Command)).SingleOrThrow(onEmpty: ("CommandNameNotFound", $"Command line {t.index} does not contain command-name."))
                let currentName = arg0.First()
                let actualName = _commands.SingleOrDefault(cmd => cmd.Name.Contains(currentName, SoftString.Comparer))?.RegistrationKey ?? throw DynamicException.Create("CommandNameNotFound", $"Command '{currentName}' not found.")
                let command = _lifetimeScope.ResolveNamed<ICommand>(actualName)
                select (command, t.args);

            var async = executables.ToLookup(e => _commandParameterBinder.Bind<CommandParameter>(e.args).Async);

            _logger.Log(Telemetry.Collect.Application().Metric("CommandCount", async.Count));
            _logger.Log(Telemetry.Collect.Application().Metric("SequentialCommandCount", async[false].Count()));
            _logger.Log(Telemetry.Collect.Application().Metric("AsyncCommandCount", async[true].Count()));

            var exceptions = new ConcurrentBag<Exception>();

            await ExecuteCommandsInSequence(async[false], context, exceptions, cancellationToken);
            await ExecuteCommandsInParallel(async[true], context, exceptions, cancellationToken);

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private async Task ExecuteCommandsInSequence<TContext>
        (
            IEnumerable<(ICommand Command, List<CommandLineArgument> Args)> executables,
            TContext context,
            ConcurrentBag<Exception> exceptions,
            CancellationToken cancellationToken
        )
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Execute sequential commands first.
            foreach (var executable in executables)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var task = ExecuteAsync(executable.Command, executable.Args, context, cts.Token);
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

        private async Task ExecuteCommandsInParallel<TContext>
        (
            IEnumerable<(ICommand Command, List<CommandLineArgument> Args)> executables,
            TContext context,
            ConcurrentBag<Exception> exceptions,
            CancellationToken cancellationToken
        )
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Now execute async commands.
            var actionBlock = new ActionBlock<(ICommand command, List<CommandLineArgument> args)>
            (
                async executable =>
                {
                    var task = ExecuteAsync(executable.command, executable.args, context, cts.Token);
                    //var commandParameterType = executable.command.ParameterType; // executable.command.GetType().GetCommandParameterType();
                    //var bindMethod = typeof(ICommandParameterBinder).GetMethod(nameof(ICommandParameterBinder.Bind))!.MakeGenericMethod(commandParameterType ?? typeof(object));
                    //var parameter = bindMethod.Invoke(_commandParameterBinder, new object[] { context, executable.args });
                    //var task = executable.command.ExecuteAsync(parameter, cts.Token);
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

            foreach (var executable in executables)
            {
                actionBlock.Post(executable);
            }

            actionBlock.Complete();
            await actionBlock.Completion;
        }

        private Task ExecuteAsync<TContext>(ICommand command, List<CommandLineArgument> args, TContext context, CancellationToken cancellationToken)
        {
            var commandParameterType = command.ParameterType;
            var bindMethod = typeof(ICommandParameterBinder).GetMethod(nameof(ICommandParameterBinder.Bind))!;
            var bindMethodGeneric = bindMethod!.MakeGenericMethod(commandParameterType ?? typeof(object));
            var parameter = bindMethodGeneric.Invoke(_commandParameterBinder, new object[] { args, context });
            return command.ExecuteAsync(parameter, cancellationToken);
        }
    }

    public static class CommandExecutorExtensions
    {
        public static Task ExecuteAsync<TContext>(this ICommandExecutor commandExecutor, IEnumerable<string> args, TContext context = default, CancellationToken cancellationToken = default)
        {
            return commandExecutor.ExecuteAsync(args.Join(" "), context, cancellationToken);
        }
    }
}