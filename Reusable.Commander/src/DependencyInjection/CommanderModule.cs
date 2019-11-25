using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Custom;
using Autofac;
using JetBrains.Annotations;
using Reusable.Commander.Utilities;
using Reusable.Exceptionize;

namespace Reusable.Commander.DependencyInjection
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module, IEnumerable<RegisterCommandDelegate>
    {
        private readonly List<RegisterCommandDelegate> _registrationActions = new List<RegisterCommandDelegate>();

        public void Add(RegisterCommandDelegate registrationAction)
        {
            _registrationActions.Add(registrationAction);
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
                .RegisterType<CommandParameterBinder>()
                .As<ICommandParameterBinder>();
            
            builder
                .RegisterType<CommandExecutor>()
                .As<ICommandExecutor>();

            var commandNames = new HashSet<MultiName>();
            foreach (var registrationAction in this)
            {
                var commandName = registrationAction(builder);
                if (!commandNames.Add(commandName))
                {
                    throw DynamicException.Create("AmbiguousCommandName", $"Command name [{commandName.Join(", ")}] is already used by another command.");
                }
            }

            builder
                .RegisterSource(new TypeListSource());
        }

        public IEnumerator<RegisterCommandDelegate> GetEnumerator() => _registrationActions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}