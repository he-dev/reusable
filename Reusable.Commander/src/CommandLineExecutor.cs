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
        Task ExecuteAsync([NotNull, ItemNotNull] IEnumerable<ICommandLine> commandLines, CancellationToken cancellationToken = default);

        Task ExecuteAsync([CanBeNull] string commandLineString, CancellationToken cancellationToken = default);
    }

    [UsedImplicitly]
    public class CommandLineExecutor : ICommandLineExecutor
    {
        private readonly ICommandLineParser _commandLineParser;
        private readonly ICommandLineMapper _mapper;
        private readonly IEnumerable<IConsoleCommand> _commands;
        private readonly ILogger _logger;

        public CommandLineExecutor
        (
            [NotNull] ILogger<CommandLineExecutor> logger,
            [NotNull] ICommandLineParser commandLineParser,
            [NotNull] ICommandLineMapper mapper,
            [NotNull] IEnumerable<IConsoleCommand> commands
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandLineParser = commandLineParser ?? throw new ArgumentNullException(nameof(commandLineParser));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public async Task ExecuteAsync(IEnumerable<ICommandLine> commandLines, CancellationToken cancellationToken)
        {
            const bool sequential = false;
            const bool async = true;

            var executables =
                _commands
                    .Find(commandLines)
                    .Select(t => (t.Command, t.CommandLine, Bag: _mapper.Map<SimpleBag>(t.CommandLine)))
                    .ToLookup(x => x.Bag.Async);

            _logger.Log(
                Abstraction.Layer.Infrastructure()
                    .Meta(
                        new
                        {
                            CommandCount = new
                            {
                                Executable = executables.Count,
                                Sequential = executables[sequential].Count(),
                                Async = executables[async].Count(),
                            }
                        }
                    )
            );

            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                // Execute sequential commands first.
                foreach (var executable in executables[sequential])
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await ExecuteAsync(executable, cts);
                }

                // Now execute the async commands.
                var tasks = executables[async].Select(async executable => await ExecuteAsync(executable, cts));
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        public async Task ExecuteAsync(string commandLineString, CancellationToken cancellationToken)
        {
            if (commandLineString.IsNullOrEmpty())
            {
                throw DynamicException.Create(
                    $"CommandStringNullOrEmpty",
                    $"You need to specify at least one command."
                );
            }

            var commandLines = _commandLineParser.Parse(commandLineString);
            await ExecuteAsync(commandLines, cancellationToken);
        }

        private async Task ExecuteAsync((IConsoleCommand Command, ICommandLine CommandLine, ICommandBag Bag) executable, CancellationTokenSource cancellationTokenSource)
        {
            using (_logger.BeginScope().WithCorrelationContext(new {Command = executable.Command.Name.FirstLongest().ToString()}).AttachElapsed())
            {
                try
                {
                    await executable.Command.ExecuteAsync(executable.CommandLine, cancellationTokenSource.Token);
                    _logger.Log(Abstraction.Layer.Infrastructure().Routine(nameof(IConsoleCommand.ExecuteAsync)).Completed());
                }
                catch (Exception taskEx)
                {
                    _logger.Log(Abstraction.Layer.Infrastructure().Routine(nameof(IConsoleCommand.ExecuteAsync)).Faulted(), taskEx);

                    if (executable.Bag.CanThrow)
                    {
                        cancellationTokenSource.Cancel();
                        throw;
                    }
#if DEBUG
                    else
                    {
                        // In debug mode (e.g. unit-testing) this should always throw. Otherwise we might hide some bugs.
                        throw DynamicException.Create(
                            $"Unexpected",
                            $"An unexpected exception occured while executing the '{executable.Command.Name.FirstLongest().ToString()}' command."
                        );
                    }
#endif
                }
            }
        }
    }
}