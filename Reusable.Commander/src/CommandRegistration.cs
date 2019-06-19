using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public static IImmutableList<CommandRegistration> Add<TCommand>
        (
            this IImmutableList<CommandRegistration> registrations,
            Action<CommandRegistration> customize = default
        ) where TCommand : ICommand
        {
            try
            {
                var registration = new CommandRegistration(typeof(TCommand), CommandHelper.GetCommandId(typeof(TCommand)));
                customize?.Invoke(registration);
                return registrations.Add(registration);
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
        public static IImmutableList<CommandRegistration> Add<TParameter>
        (
            this IImmutableList<CommandRegistration> registrations,
            [NotNull] Identifier id,
            [NotNull] ExecuteCallback<TParameter> execute
        ) where TParameter : ICommandParameter
        {
            return registrations.Add<Lambda<TParameter>>(r =>
            {
                r.Id = id;
                r.Execute = execute;
            });
        }
    }

    public delegate void CustomizeCommandRegistrationCallback(IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> customize);

    [UsedImplicitly]
    [PublicAPI]
    public class CommandRegistration
    {
        internal CommandRegistration(Type type, Identifier id)
        {
            //new CommandValidator().ValidateCommand((type, id));
            Type = type;
            Id = id;
        }

        [NotNull]
        public Type Type { get; }

        [NotNull]
        public Identifier Id { get; set; }

        [CanBeNull]
        public object Execute { get; set; }

        [CanBeNull]
        public CustomizeCommandRegistrationCallback Customize { get; set; }

        internal void Register([NotNull] ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (Id is null) throw new InvalidOperationException($"{nameof(Id)} must not be null.");

            var registration =
                builder
                    .RegisterType(Type)
                    .Keyed<ICommand>(Id)
                    .As<ICommand>();

            // Lambda command ctor has some extra properties that we need to set.
            if (!(Execute is null))
            {
                registration
                    .WithParameter(new NamedParameter("id", Id))
                    .WithParameter(new NamedParameter("execute", Execute));
            }

            Customize?.Invoke(registration);
        }
    }
}