using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Reusable.Converters;
using Reusable.OmniLog;
using Reusable.Reflection;

namespace Reusable.Commander
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module
    {
        
        private readonly IEnumerable<Type> _commandTypes;
        private readonly ITypeConverter _converter;

        public CommanderModule([NotNull] IEnumerable<Type> commandTypes, [CanBeNull] ITypeConverter converter = null)
        {
            if (commandTypes == null) throw new ArgumentNullException(nameof(commandTypes));
            
            _commandTypes = commandTypes.ToList();
            _converter = converter ?? CommandLineMapper.DefaultConverter;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            var commandValidator = new CommandValidator();
            foreach (var commandType in _commandTypes)
            {
                commandValidator.ValidateCommand(commandType, _converter);                             
                builder
                    .RegisterType(commandType)
                    .Keyed<IConsoleCommand>(CommandHelper.GetCommandName(commandType))
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
                .WithParameter(new TypedParameter(typeof(ITypeConverter), _converter))
                .As<ICommandLineMapper>();
            
            builder
                .RegisterType<CommandLineExecutor>()
                .As<ICommandLineExecutor>();
        }
    }
}