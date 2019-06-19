using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Reusable.Commander.Commands;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OneTo1;

namespace Reusable.Commander
{
    /// <summary>
    /// This class allows to easily register and validate commands.
    /// </summary>
    public static class ImmutableListExtensions
    {
        public static IImmutableList<CommandModule> Add<TCommand>
        (
            this IImmutableList<CommandModule> registrations,
            ConfigureRegistrationCallback configureRegistrationCallback = default
        ) where TCommand : ICommand
        {
            try
            {
                return registrations.Add(new CommandModule
                (
                    typeof(TCommand),
                    CommandHelper.GetCommandId(typeof(TCommand)),
                    configureRegistrationCallback
                ));
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    $"RegisterCommand",
                    $"An error occured while trying to register '{typeof(TCommand).ToPrettyString()}'. See the inner-exception for details.",
                    inner
                );
            }
        }

        [NotNull]
        public static IImmutableList<CommandModule> Add<TParameter, TContext>
        (
            this IImmutableList<CommandModule> registrations,
            [NotNull] Identifier id,
            [NotNull] ExecuteCallback<TParameter, TContext> execute,
            ConfigureRegistrationCallback configureRegistrationCallback = default
        ) where TParameter : ICommandArgumentGroup
        {
            return registrations.Add(new CommandModule<TParameter, TContext>
            (
                typeof(Lambda<TParameter, TContext>),
                id,
                execute,
                configureRegistrationCallback
            ));
        }
    }

    public delegate void ConfigureRegistrationCallback(IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> configure);

    public class CommandModule : Autofac.Module
    {
        internal CommandModule
        (
            Type type,
            Identifier id,
            ConfigureRegistrationCallback configureRegistrationCallback = default
        )
        {
            //new CommandValidator().ValidateCommand((type, id));
            Type = type;
            Id = id;
            ConfigureRegistrationCallback = configureRegistrationCallback;
        }

        [NotNull]
        private Type Type { get; }

        [NotNull]
        private Identifier Id { get; }

        [CanBeNull]
        private ConfigureRegistrationCallback ConfigureRegistrationCallback { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            var registration =
                builder
                    .RegisterType(Type)
                    .Keyed<ICommand>(Id)
                    .As<ICommand>();

            ConfigureRegistrationCallback?.Invoke(registration);
        }
    }

    [UsedImplicitly]
    [PublicAPI]
    public class CommandModule<TParameter, TContext> : CommandModule where TParameter : ICommandArgumentGroup
    {
        internal CommandModule
        (
            Type type,
            Identifier id,
            ExecuteCallback<TParameter, TContext> executeCallback,
            ConfigureRegistrationCallback configureRegistrationCallback = default
        ) : base(type, id, builder =>
        {
            builder
                .WithParameter(new TypedParameter(typeof(Identifier), id))
                .WithParameter(new TypedParameter(typeof(ExecuteCallback<TParameter, TContext>), executeCallback));

            configureRegistrationCallback?.Invoke(builder);
        }) { }
    }
}