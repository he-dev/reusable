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
using Reusable.Converters;
using Reusable.OmniLog;
using Reusable.Reflection;

namespace Reusable.Commander
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module
    {
        [NotNull]
        private readonly ITypeConverter _parameterConverter;

        [NotNull]
        private readonly CommandRegistrationBuilder _commands;

        public CommanderModule([NotNull] Action<CommandRegistrationBuilder> commands, [NotNull] ITypeConverter parameterConverter)
        {
            _parameterConverter = parameterConverter ?? throw new ArgumentNullException(nameof(parameterConverter));
            _commands = new CommandRegistrationBuilder(parameterConverter);
            commands(_commands);
        }

        public CommanderModule([NotNull] Action<CommandRegistrationBuilder> commands) : this(commands, CommandLineMapper.DefaultConverter) { }

        protected override void Load(ContainerBuilder builder)
        {
            foreach (var command in _commands)
            {
                var registration = builder.RegisterType(command.Type);

                if (!(command.Execute is null))
                {
                    registration.WithParameter(new NamedParameter("name", command.Name));
                    registration.WithParameter(command.Execute);
                }

                registration
                    .Keyed<IConsoleCommand>(command.Name)
                    .As<IConsoleCommand>();
            }

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
        }
    }

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

        public CommandRegistrationBuilder Add<T>() where T : IConsoleCommand => Add((typeof(T), CommandHelper.GetCommandName(typeof(T)), default));

        public CommandRegistrationBuilder Add<T>([NotNull] SoftKeySet name) where T : IConsoleCommand => Add((typeof(T), name ?? throw new ArgumentNullException(nameof(name)), default));

        public CommandRegistrationBuilder Add<T>([NotNull] SoftKeySet name, LambdaExecuteCallback<T> execute) where T : ICommandBag, new()
        {
            return Add(
                (
                    typeof(Lambda<T>),
                    name ?? throw new ArgumentNullException(nameof(name)),
                    new NamedParameter("execute", execute)
                )
            );
        }

        private CommandRegistrationBuilder Add((Type Type, SoftKeySet Name, NamedParameter execute) command)
        {
            _validator.ValidateCommand((command.Type, command.Name), _parameterConverter);
            _commands.Add(new CommandRegistrationBuilderItem(command));
            return this;
        }

    #region IEnumerable

        public IEnumerator<CommandRegistrationBuilderItem> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
    }

    public class CommandRegistrationBuilderItem
    {
        internal CommandRegistrationBuilderItem((Type Type, SoftKeySet Name, NamedParameter Execute) command)
        {
            (Type, Name, Execute) = command;
        }

        public Type Type { get; }

        public SoftKeySet Name { get; }

        public NamedParameter Execute { get; }
    }
}