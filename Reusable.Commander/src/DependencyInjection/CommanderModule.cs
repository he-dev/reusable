using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Autofac;
using JetBrains.Annotations;
using Reusable.Commander.Utilities;

namespace Reusable.Commander.DependencyInjection
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module, IEnumerable<Action<ContainerBuilder>>
    {
        private readonly List<Action<ContainerBuilder>> _registrationActions = new List<Action<ContainerBuilder>>();

        public void Add(Action<ContainerBuilder> registrationAction) => _registrationActions.Add(registrationAction);

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

            foreach (var registrationAction in this)
            {
                registrationAction(builder);
            }

            builder
                .RegisterSource(new TypeListSource());
        }

        public IEnumerator<Action<ContainerBuilder>> GetEnumerator() => _registrationActions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}