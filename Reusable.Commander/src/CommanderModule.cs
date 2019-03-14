using System;
using System.CodeDom;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Features.AttributeFilters;
using JetBrains.Annotations;
using Reusable.Commander.Utilities;
using Reusable.OmniLog;
using Reusable.OneTo1;

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

        public CommanderModule([NotNull] Action<CommandRegistrationBuilder> commands)
            : this(commands, CommandLineMapper.DefaultConverter) { }

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
                .RegisterInstance((ExecuteExceptionCallback)(_ => { }));

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

                // Lambda command ctor has some extra properties that we need to set.
                if (command.IsLambda)
                {
                    registration.WithParameter(new NamedParameter("id", command.Id));
                    registration.WithParameter(command.Execute);
                }

                command.Customize(registration);
            }

            builder
                .RegisterSource(new TypeListSource<IConsoleCommand>());
        }
    }
}