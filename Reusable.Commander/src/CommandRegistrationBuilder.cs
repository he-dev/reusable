using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Reusable.Commander.Commands;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Reflection;

namespace Reusable.Commander
{
    /// <summary>
    /// This class allows to easily register and validate commands.
    /// </summary>
    public class CommandRegistrationBuilder : Autofac.Module
    {
        private readonly ITypeConverter _parameterConverter;
        private readonly IList<CommandRegistration> _commands;

        internal CommandRegistrationBuilder(ITypeConverter parameterConverter)
        {
            _parameterConverter = parameterConverter;
            _commands = new List<CommandRegistration>();
        }

        [NotNull]
        public CommandRegistrationBuilder Add<TCommand>(Action<CommandRegistration> customize = default) where TCommand : IConsoleCommand
        {
            try
            {
                var registration = new CommandRegistration(_parameterConverter, typeof(TCommand), CommandHelper.GetCommandId(typeof(TCommand)));
                customize?.Invoke(registration);
                _commands.Add(registration);
                return this;
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
        public CommandRegistrationBuilder Add<TBag>([NotNull] Identifier id, [NotNull] ExecuteCallback<TBag> execute) where TBag : ICommandBag, new()
        {
            return Add<Lambda<TBag>>(r =>
            {
                r.Id = id;
                r.Execute = execute;
            });
        }

        protected override void Load(ContainerBuilder builder)
        {
            foreach (var command in _commands)
            {
                command.RegisterWith(builder);                
            }
        }
    }

    public delegate void CustomizeCommandRegistrationCallback(IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> customize);

    [UsedImplicitly]
    [PublicAPI]
    public class CommandRegistration
    {
        internal CommandRegistration(ITypeConverter parameterConverter, Type type, Identifier id)
        {
            new CommandValidator().ValidateCommand((type, id), parameterConverter);
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

        internal void RegisterWith([NotNull] ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (Id is null) throw new InvalidOperationException($"{nameof(Id)} must not be null.");

            var registration =
                builder
                    .RegisterType(Type)
                    .Keyed<IConsoleCommand>(Id)
                    .As<IConsoleCommand>();

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