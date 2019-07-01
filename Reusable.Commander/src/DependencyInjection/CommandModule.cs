using System;
using Autofac;
using JetBrains.Annotations;
using Reusable.Commander.Commands;

namespace Reusable.Commander.DependencyInjection
{
    public class CommandModule : Autofac.Module
    {
        internal CommandModule
        (
            Type type,
            NameSet id,
            ConfigureRegistrationCallback configureRegistrationCallback = default
        )
        {
            Validator.ValidateCommand(type, CommandArgumentConverter.Default);

            Type = type;
            Id = id;
            ConfigureRegistrationCallback = configureRegistrationCallback;
        }

        [NotNull]
        private Type Type { get; }

        [NotNull]
        private NameSet Id { get; }

        [CanBeNull]
        private ConfigureRegistrationCallback ConfigureRegistrationCallback { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            var registration =
                builder
                    .RegisterType(Type)
                    .SingleInstance()
                    .As<ICommand>();

            ConfigureRegistrationCallback?.Invoke(registration);
        }
    }

    [UsedImplicitly]
    [PublicAPI]
    public class CommandModule<TCommandLine, TContext> : CommandModule where TCommandLine : ICommandLine
    {
        internal CommandModule
        (
            Type type,
            NameSet id,
            ExecuteCallback<TCommandLine, TContext> executeCallback,
            ConfigureRegistrationCallback configureRegistrationCallback = default
        ) : base(type, id, builder =>
        {
            builder
                .WithParameter(new TypedParameter(typeof(NameSet), id))
                .WithParameter(new TypedParameter(typeof(ExecuteCallback<TCommandLine, TContext>), executeCallback));

            configureRegistrationCallback?.Invoke(builder);
        }) { }
    }
}