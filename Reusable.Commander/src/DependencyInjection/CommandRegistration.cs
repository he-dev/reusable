using System;
using System.Collections.Immutable;
using Autofac.Builder;
using JetBrains.Annotations;
using Reusable.Commander.Commands;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Commander.DependencyInjection
{
    /// <summary>
    /// This class allows to easily register and validate commands.
    /// </summary>
    public static class CommandRegistration
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
        public static IImmutableList<CommandModule> Add<TCommandLine, TContext>
        (
            this IImmutableList<CommandModule> registrations,
            [NotNull] NameSet id,
            [NotNull] ExecuteCallback<TCommandLine, TContext> execute,
            ConfigureRegistrationCallback configureRegistrationCallback = default
        ) where TCommandLine : class, ICommandLine
        {
            return registrations.Add(new CommandModule<TCommandLine, TContext>
            (
                typeof(Lambda<TCommandLine, TContext>),
                id,
                execute,
                configureRegistrationCallback
            ));
        }
    }

    public delegate void ConfigureRegistrationCallback(IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> configure);
}