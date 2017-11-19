using System;
using Autofac;
using JetBrains.Annotations;
using Reusable.Converters;
using Reusable.OmniLog;

namespace Reusable.Commander
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module
    {
        private readonly ICommandRegistrationContainer _registrations;

        public CommanderModule([NotNull] ICommandRegistrationContainer registrations)
        {
            _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
        }

        public ITypeConverter Converter { get; set; } = CommandParameterMapper.DefaultConverter;
        
        protected override void Load(ContainerBuilder builder)
        {
            foreach (var registration in _registrations)
            {
                builder
                    .RegisterType(registration.CommandType)
                    .Keyed<IConsoleCommand>(registration.CommandName)
                    .As<IConsoleCommand>();
            }
            
            builder
                .RegisterInstance(_registrations);
            
            builder
                .RegisterType<CommandLineTokenizer>()
                .As<ICommandLineTokenizer>();

            builder
                .RegisterType<CommandLineParser>()
                .As<ICommandLineParser>();

            builder
                .RegisterType<CommandParameterMapper>()
                .WithParameter(new TypedParameter(typeof(ITypeConverter), Converter ?? throw new InvalidOperationException($"{nameof(Converter)} must not be null.")))
                .As<ICommandParameterMapper>();

            builder
                .RegisterType<CommandFactory>()
                .As<ICommandFactory>();
            
            builder
                .RegisterType<CommandLineExecutor>()
                .As<ICommandLineExecutor>();
        }
    }
}