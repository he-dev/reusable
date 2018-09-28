using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface IConsoleCommand
    {
        [NotNull]
        SoftKeySet Name { get; }

        Task ExecuteAsync([CanBeNull] object parameter, CancellationToken cancellationToken = default);
    }

    public abstract class ConsoleCommand<TBag> : IConsoleCommand where TBag : ICommandBag, new()
    {
        private readonly ICommandLineMapper _mapper;

        protected ConsoleCommand([NotNull] ILogger logger, [NotNull] ICommandLineMapper mapper, SoftKeySet name) : this(logger, mapper)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            Name = name;
        }

        protected ConsoleCommand([NotNull] ILogger logger, [NotNull] ICommandLineMapper mapper)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            Name = CommandHelper.GetCommandName(GetType());
        }

        [NotNull]
        protected ILogger Logger { get; }

        public SoftKeySet Name { get; }

        public async Task ExecuteAsync(object parameter, CancellationToken cancellationToken)
        {
            switch (parameter)
            {
                case null:
                    await ExecuteAsync(default, cancellationToken);
                    break;

                case ICommandLine commandLine:
                    await ExecuteAsync(_mapper.Map<TBag>(commandLine), cancellationToken);
                    break;

                case TBag bag:
                    await ExecuteAsync(bag, cancellationToken);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        paramName: nameof(parameter),
                        message: $"{nameof(parameter)} must be either a {typeof(ICommandLine).Name} or {typeof(TBag).Name}."
                    );
            }
        }

        protected abstract Task ExecuteAsync(TBag parameter, CancellationToken cancellationToken);
    }
}