using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            Action<CommandModule> customize = default
        ) where TCommand : ICommand
        {
            try
            {
                var registration = new CommandModule(typeof(TCommand), CommandHelper.GetCommandId(typeof(TCommand)));
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
        public static IImmutableList<CommandModule> Add<TParameter, TContext>
        (
            this IImmutableList<CommandModule> registrations,
            [NotNull] IEnumerable<string> names,
            [NotNull] ExecuteCallback<TParameter, TContext> execute
        ) where TParameter : ICommandParameter
        {
            var _names = new[] { new Name(names.First(), NameOption.CommandLine), }.Concat(names.Skip(1).Select(n => new Name(n, NameOption.Alias)));
            return registrations.Add<Lambda<TParameter, TContext>>(r =>
            {
                r.Id = new Identifier(_names);
                r.Execute = execute;
            });
        }
    }

    public delegate void ConfigureRegistrationCallback(IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> configure);

    [UsedImplicitly]
    [PublicAPI]
    public class CommandModule<TParameter, TContext> : Autofac.Module where TParameter : ICommandParameter
    {
        internal CommandModule
        (
            Type type,
            Identifier id,
            ExecuteCallback<TParameter, TContext> executeCallback = default,
            ConfigureRegistrationCallback configureRegistrationCallback = default
        )
        {
            //new CommandValidator().ValidateCommand((type, id));
            Type = type;
            Id = id;
        }

        [NotNull]
        private Type Type { get; }

        [NotNull]
        private Identifier Id { get; set; }

        [CanBeNull]
        private ExecuteCallback<TParameter, TContext> ExecuteCallback { get; set; }

        [CanBeNull]
        private ConfigureRegistrationCallback ConfigureRegistrationCallback { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            var registration =
                builder
                    .RegisterType(Type)
                    .Keyed<ICommand>(Id)
                    .As<ICommand>();

            // Lambda command ctor has some extra properties that we need to set.
            if (!(ExecuteCallback is null))
            {
                registration
                    .WithParameter(new NamedParameter("id", Id))
                    .WithParameter(new NamedParameter("execute", ExecuteCallback));
            }

            ConfigureRegistrationCallback?.Invoke(registration);
        }
    }
}