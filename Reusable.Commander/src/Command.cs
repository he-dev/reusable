using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander.DependencyInjection;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;
using Autofac;
using Autofac.Builder;
using Reusable.Commander.Commands;

namespace Reusable.Commander
{
    public interface ICommand
    {
        MultiName Name { get; }

        Task ExecuteAsync(object? parameter, CancellationToken cancellationToken = default);
    }

    [PublicAPI]
    public abstract class Command<TParameter> : ICommand where TParameter : class, new()
    {
        protected Command(ILogger logger, MultiName? name = default)
        {
            Logger = logger;
            Name = name ?? CommandHelper.GetMultiName(GetType());
        }

        protected ILogger Logger { get; }

        public virtual MultiName Name { get; }

        public virtual async Task ExecuteAsync(object? parameter, CancellationToken cancellationToken)
        {
            await ExecuteAsync(parameter as TParameter, cancellationToken);
        }

        protected abstract Task ExecuteAsync(TParameter? parameter, CancellationToken cancellationToken);
    }

    public static class Command
    {
        public static Action<ContainerBuilder> Registration<T>
        (
            Action<IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>>? configure = default
        )
        {
            return builder =>
            {
                var registration = builder.RegisterType<T>();
                configure?.Invoke(registration);
            };
        }

        public static Action<ContainerBuilder> Registration<TParameter>
        (
            MultiName name,
            ExecuteDelegate<TParameter> execute,
            Action<IRegistrationBuilder<Lambda<TParameter>, SimpleActivatorData, SingleRegistrationStyle>>? configure = default
        ) where TParameter : class, new()
        {
            return builder =>
            {
                var registration = builder.Register(ctx => new Lambda<TParameter>(ctx.Resolve<ILogger<Lambda<TParameter>>>(), name, execute));
                configure?.Invoke(registration);
            };
        }
    }

    //public delegate void ConfigureRegistrationDelegate<in T>(IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> configure);

    //public delegate void ConfigureRegistrationDelegate<in T>(IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> configure);

    [UsedImplicitly]
    [PublicAPI]
    public abstract class Decorator : ICommand
    {
        protected Decorator(ICommand command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        [NotNull]
        protected ICommand Command { get; }

        public virtual MultiName Name => Command.Name;

        public abstract Task ExecuteAsync(object? parameter, CancellationToken cancellationToken);
    }

    public class Logger : Decorator
    {
        private readonly ILogger<Logger> _logger;

        public Logger(ICommand command, ILogger<Logger> logger) : base(command)
        {
            _logger = logger;
        }

        public override async Task ExecuteAsync(object? parameter, CancellationToken cancellationToken)
        {
            using (_logger.BeginScope().WithCorrelationHandle("ExecuteCommand").UseStopwatch())
            {
                _logger.Log(Abstraction.Layer.Service().Meta(new { CommandName = Command.Name.First() }).Trace());
                try
                {
                    await Command.ExecuteAsync(parameter, cancellationToken);
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Completed());
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Canceled(), "Cancelled.");
                    throw;
                }
                catch (Exception taskEx)
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Faulted(), taskEx);
                    throw;
                }
            }
        }
    }
}