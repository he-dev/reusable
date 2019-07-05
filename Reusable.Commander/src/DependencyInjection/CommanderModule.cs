using System;
using System.Collections.Immutable;
using Autofac;
using JetBrains.Annotations;
using Reusable.Commander.Utilities;

namespace Reusable.Commander.DependencyInjection
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module
    {
        [NotNull] private readonly IImmutableList<CommandModule> _commandModules;

        public CommanderModule([NotNull] IImmutableList<CommandModule> commandModules)
        {
            _commandModules = commandModules ?? throw new ArgumentNullException(nameof(commandModules));
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
                .RegisterType<CommandFactory>()
                .SingleInstance()
                .As<ICommandFactory>();

            builder
                .RegisterType<CommandExecutor>()
                .As<ICommandExecutor>();
            
            foreach (var commandModule in _commandModules)
            {
               builder.RegisterModule(commandModule);
            }

            builder
                .RegisterSource(new TypeListSource());
        }
    }
}