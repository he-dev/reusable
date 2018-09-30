using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Reusable.Commander.Commands;
using Reusable.Commander.Utilities;
using Reusable.Converters;
using Reusable.OmniLog;
using Reusable.Reflection;

namespace Reusable.Commander
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module
    {
        [NotNull] private readonly ITypeConverter _parameterConverter;

        [NotNull] private readonly CommandRegistrationBuilder _commands;

        public CommanderModule([NotNull] Action<CommandRegistrationBuilder> commands, [NotNull] ITypeConverter parameterConverter)
        {
            _parameterConverter = parameterConverter ?? throw new ArgumentNullException(nameof(parameterConverter));
            _commands = new CommandRegistrationBuilder(parameterConverter);
            if (commands is null) throw new ArgumentNullException(nameof(commands));
            commands(_commands);
        }

        public CommanderModule([NotNull] Action<CommandRegistrationBuilder> commands) : this(commands, CommandLineMapper.DefaultConverter)
        {
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<CommandLineTokenizer>()
                .As<ICommandLineTokenizer>();

            builder
                .RegisterType<CommandLineParser>()
                .As<ICommandLineParser>();

            builder
                .RegisterType<CommandLineMapper>()
                .WithParameter(new TypedParameter(typeof(ITypeConverter), _parameterConverter))
                .As<ICommandLineMapper>();

            builder
                .RegisterType<CommandLineExecutor>()
                .As<ICommandLineExecutor>();

            builder
                .RegisterGeneric(typeof(CommandServiceProvider<>));
            
            foreach (var command in _commands)
            {
                var registration =
                    builder
                        .RegisterType(command.Type)
                        .Keyed<IConsoleCommand>(command.Id)
                        .As<IConsoleCommand>();

                // Lambda command ctor have some extra properties that we need to set.
                if (command.IsLambda)
                {
                    registration.WithParameter(new NamedParameter("id", command.Id));
                    registration.WithParameter(command.Execute);
                }
            }
            
            builder
                .RegisterSource(new TypeListSource<IConsoleCommand>());
        }
    }

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
        public CommandRegistrationBuilder Add<T>() where T : IConsoleCommand
        {
            return Add((typeof(T), CommandHelper.GetCommandId(typeof(T)), default));
        }

        [NotNull]
        public CommandRegistrationBuilder Add<T>([NotNull] Identifier id) where T : IConsoleCommand
        {
            return Add((typeof(T), id ?? throw new ArgumentNullException(nameof(id)), default));
        }

        [NotNull]
        public CommandRegistrationBuilder Add<T>([NotNull] Identifier id, [NotNull] ExecuteCallback<T> execute) where T : ICommandBag, new()
        {
            return Add(
                (
                    typeof(Lambda<T>),
                    id ?? throw new ArgumentNullException(nameof(id)),
                    new NamedParameter("execute", execute ?? throw new ArgumentNullException(nameof(execute)))
                )
            );
        }

        [NotNull]
        private CommandRegistrationBuilder Add((Type Type, Identifier Id, NamedParameter execute) command)
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

    public class CommandRegistrationBuilderItem
    {
        internal CommandRegistrationBuilderItem((Type Type, Identifier Id, NamedParameter Execute) command)
        {
            (Type, Id, Execute) = command;
        }

        public Type Type { get; }

        public Identifier Id { get; }

        public NamedParameter Execute { get; }

        public bool IsLambda => !(Execute is null);
    }
}