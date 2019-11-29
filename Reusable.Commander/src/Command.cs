using System;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Autofac;
using Autofac.Builder;
using Reusable.Commander.Annotations;
using Reusable.Commander.Commands;
using Reusable.Exceptionize;

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
            Name = name ?? GetType().GetMultiName();
        }

        protected ILogger Logger { get; }

        public virtual MultiName Name { get; }

        public virtual async Task ExecuteAsync(object? parameter, CancellationToken cancellationToken)
        {
            await ExecuteAsync
            (
                parameter as TParameter ?? throw new ArgumentOutOfRangeException(paramName: nameof(parameter), message: $"{Name} command parameter must be of type {typeof(TParameter).ToPrettyString()}."),
                cancellationToken
            );
        }

        protected abstract Task ExecuteAsync(TParameter? parameter, CancellationToken cancellationToken);
    }

    public static class Command
    {
        public static RegisterCommandDelegate Registration<T>
        (
            Action<IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle>>? configure = default
        )
        {
            try
            {
                ValidateParameterPropertyNames(typeof(T).GetCommandParameterType());

                return builder =>
                {
                    var registration = builder.RegisterType<T>();
                    configure?.Invoke(registration);
                    return typeof(T).GetMultiName();
                };
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("CommandRegistration", $"Command {typeof(T).GetMultiName().Join(", ").EncloseWith("[]")} could not be registered. See the inner exception for details.", inner);
            }
        }

        public static RegisterCommandDelegate Registration<TParameter>
        (
            MultiName name,
            ExecuteDelegate<TParameter> execute,
            Action<IRegistrationBuilder<Lambda<TParameter>, SimpleActivatorData, SingleRegistrationStyle>>? configure = default
        ) where TParameter : CommandParameter, new()
        {
            ValidateParameterPropertyNames(typeof(TParameter));

            return builder =>
            {
                var registration = builder.Register(ctx => new Lambda<TParameter>(ctx.Resolve<ILogger<Lambda<TParameter>>>(), name, execute)).As<ICommand>();
                configure?.Invoke(registration);
                return name;
            };
        }

        private static void ValidateParameter(Type parameterType)
        {
            try
            {
                ValidateParameterPropertyNames(parameterType);
                ValidateParameterPropertyPositions(parameterType);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("ParameterValidation", $"Parameter {parameterType.GetMultiName().Join(", ").EndsWith("[]")} is invalid. See the inner exception for details.", inner);
            }
        }

        private static void ValidateParameterPropertyNames(Type parameterType)
        {
            var query =
                from p in parameterType.GetParameterProperties()
                from n in p.GetMultiName()
                group n by n into g
                where g.Count() > 1
                select g.Key;

            if (query.ToList() is var duplicates && duplicates.Any())
            {
                throw DynamicException.Create($"AmbiguousArgumentName", $"Some argument names are used multiple times: {duplicates.Join(", ").EncloseWith("[]")}");
            }
        }

        private static void ValidateParameterPropertyPositions(Type parameterType)
        {
            var positions = parameterType.GetParameterProperties().Select(p => p.GetCustomAttribute<PositionAttribute>()).Where(p => p is {}).Select(p => p.Value);
            var positionCount = parameterType.GetParameterProperties().Count(p => p.IsDefined(typeof(PositionAttribute)));
            positionCount = positionCount.IsEven() ? positionCount : positionCount + 1;
            var pairCount = positionCount / 2;
            var expectedSum = (1 + positionCount) * pairCount;
            var actualSum = positions.Sum();
            if (actualSum != expectedSum)
            {
                throw DynamicException.Create($"NonContinuousArgumentPosition", "Argument positions must start with 1 and be continuous.");
            }
        }
    }

    public delegate MultiName RegisterCommandDelegate(ContainerBuilder builder);

    //public delegate void ConfigureRegistrationDelegate<in T>(IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> configure);

    //public delegate void ConfigureRegistrationDelegate<in T>(IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> configure);
}