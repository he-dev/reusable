using System;
using System.Collections.Immutable;
using Autofac;
using JetBrains.Annotations;
using Reusable.Commander.Services;
using Reusable.Commander.Utilities;
using Reusable.OneTo1;

namespace Reusable.Commander
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module
    {
        //[NotNull] private readonly ITypeConverter _parameterConverter;

        [NotNull] private readonly IImmutableList<CommandRegistration> _commandRegistrations;

        public CommanderModule([NotNull] IImmutableList<CommandRegistration> commandRegistrations)
        {
            _commandRegistrations = commandRegistrations ?? throw new ArgumentNullException(nameof(commandRegistrations));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<CommandLineTokenizer>()
                .As<ICommandLineTokenizer>();

            builder
                .RegisterType<CommandLineParser>()
                .As<ICommandLineParser>();

            // builder
            //     .RegisterType<CommandLineMapper>()
            //     .WithParameter(new TypedParameter(typeof(ITypeConverter), _parameterConverter))
            //     .As<ICommandLineMapper>();

            builder
                .RegisterInstance((ExecuteExceptionCallback)(_ => { }));

            builder
                .RegisterType<CommandExecutor>()
                .As<ICommandExecutor>();

            builder
                .RegisterGeneric(typeof(CommandServiceProvider<>));


            // builder
            //     .RegisterModule(_commandRegistrations);
            
            foreach (var commandRegistration in _commandRegistrations)
            {
                commandRegistration.Register(builder);
            }

            builder
                .RegisterSource(new TypeListSource());
        }
    }
}