using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Commander.DependencyInjection
{
    [PublicAPI]
    public class CommandModule : Autofac.Module
    {
        private readonly Action<ICommandRegistrationBuilder> _build;

        public CommandModule(Action<ICommandRegistrationBuilder> build) => _build = build;

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<CommandLineTokenizer>()
                .As<ICommandLineTokenizer>();

            builder
                .RegisterType<CommandLineParser>()
                .As<ICommandLineParser>();

            // builder
            //     .RegisterType<CommandFactory>()
            //     .SingleInstance()
            //     .As<ICommandFactory>();

            builder
                .RegisterType<CommandParameterBinder>()
                .As<ICommandParameterBinder>();

            builder
                .RegisterType<CommandExecutor>()
                .As<ICommandExecutor>();

            var crb = new CommandRegistrationBuilder { Builder = builder }.Also(_build);

            builder
                .RegisterInstance(crb.ToList())
                .As<IEnumerable<CommandInfo>>();

            // builder
            //     .RegisterSource(new TypeListSource());
        }
    }
}