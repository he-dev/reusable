using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Autofac;
using JetBrains.Annotations;
using Reusable.Converters;
using Reusable.OmniLog;

namespace Reusable.Commander
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module
    {
        private readonly IEnumerable<Type> _commandTypes;

        public CommanderModule([NotNull] IEnumerable<Type> commandTypes)
        {
            _commandTypes = commandTypes ?? throw new ArgumentNullException(nameof(commandTypes));
        }

        //public ITypeConverter Converter { get; set; } = CommandParameterMapper.DefaultConverter;
        
        protected override void Load(ContainerBuilder builder)
        {
            foreach (var commandType in _commandTypes)
            {
                CommandParameterValidator.ValidateCommandBagPropertyUniqueness(commandType);
                
                builder
                    .RegisterType(commandType)
                    .Keyed<IConsoleCommand>(NameFactory.CreateCommandName(commandType))
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
                //.WithParameter(new TypedParameter(typeof(ITypeConverter), Converter ?? throw new InvalidOperationException($"{nameof(Converter)} must not be null.")))
                .WithParameter(new TypedParameter(typeof(ITypeConverter), CommandLineMapper.DefaultConverter))
                .As<ICommandLineMapper>();

            //builder
            //    .RegisterType<CommandFactory>()
            //    .As<ICommandFactory>();
            
            builder
                .RegisterType<CommandLineExecutor>()
                .As<ICommandLineExecutor>();
        }
    }
}