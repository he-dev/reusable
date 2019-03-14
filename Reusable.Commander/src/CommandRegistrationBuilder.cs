using System;
using System.Collections;
using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Reusable.Commander.Commands;
using Reusable.Exceptionizer;
using Reusable.OneTo1;
using Reusable.Reflection;

namespace Reusable.Commander
{
    /// <summary>
    /// This class allows to easily register and validate commands.
    /// </summary>
    public class CommandRegistrationBuilder : IEnumerable<CommandRegistrationBuilderItem>
    {
        private readonly ITypeConverter _parameterConverter;
        private readonly IList<CommandRegistrationBuilderItem> _commands;
        private readonly CommandValidator _validator;

        internal CommandRegistrationBuilder([NotNull] ITypeConverter parameterConverter)
        {
            _parameterConverter = parameterConverter;
            _commands = new List<CommandRegistrationBuilderItem>();
            _validator = new CommandValidator();
        }

        [NotNull]
        public CommandRegistrationBuilder Add<TCommand>(CustomizeCommandRegistrationCallback customize = default) where TCommand : IConsoleCommand
        {
            return Add((typeof(TCommand), CommandHelper.GetCommandId(typeof(TCommand)), default, customize));
        }

        [NotNull]
        public CommandRegistrationBuilder Add<TCommand>([NotNull] Identifier id, CustomizeCommandRegistrationCallback customize = default) where TCommand : IConsoleCommand
        {
            return Add((typeof(TCommand), id ?? throw new ArgumentNullException(nameof(id)), default, customize));
        }

        [NotNull]
        public CommandRegistrationBuilder Add<TBag>([NotNull] Identifier id, [NotNull] ExecuteCallback<TBag> execute) where TBag : ICommandBag, new()
        {
            return Add
            ((
                typeof(Lambda<TBag>),
                id ?? throw new ArgumentNullException(nameof(id)),
                new NamedParameter("execute", execute ?? throw new ArgumentNullException(nameof(execute))),
                default
            ));
        }

        [NotNull]
        private CommandRegistrationBuilder Add((Type Type, Identifier Id, NamedParameter execute, CustomizeCommandRegistrationCallback Customize) command)
        {
            try
            {
                _validator.ValidateCommand((command.Type, command.Id), _parameterConverter);
                _commands.Add(new CommandRegistrationBuilderItem(command));
                return this;
            }
            catch (Exception innerException)
            {
                throw DynamicException.Create(
                    $"RegisterCommand",
                    $"An error occured while trying to register the '{command.Id.Default.ToString()}' command. See the inner-exception for details.",
                    innerException
                );
            }
        }

        #region IEnumerable

        public IEnumerator<CommandRegistrationBuilderItem> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    public delegate void CustomizeCommandRegistrationCallback(IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> customize);

    public class CommandRegistrationBuilderItem
    {
        internal CommandRegistrationBuilderItem((Type Type, Identifier Id, NamedParameter Execute, CustomizeCommandRegistrationCallback Customize) command)
        {
            (Type, Id, Execute, Customize) = (command.Type, command.Id, command.Execute, command.Customize ?? (b => { }));
        }

        [NotNull]
        public Type Type { get; }

        [NotNull]
        public Identifier Id { get; }

        [CanBeNull]
        public NamedParameter Execute { get; }

        [NotNull]
        public CustomizeCommandRegistrationCallback Customize { get; }

        public bool IsLambda => !(Execute is null);
    }
}