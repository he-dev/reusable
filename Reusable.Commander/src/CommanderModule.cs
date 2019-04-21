using System;
using Autofac;
using JetBrains.Annotations;
using Reusable.Commander.Utilities;
using Reusable.OneTo1;

namespace Reusable.Commander
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module
    {
        [NotNull] private readonly ITypeConverter _parameterConverter;

        [NotNull] private readonly CommandRegistrationBuilder _registrations;

        public CommanderModule([NotNull] Action<CommandRegistrationBuilder> register, [NotNull] ITypeConverter parameterConverter)
        {
            if (register is null) throw new ArgumentNullException(nameof(register));
            
            _parameterConverter = parameterConverter ?? throw new ArgumentNullException(nameof(parameterConverter));
            _registrations = new CommandRegistrationBuilder(parameterConverter);
            register(_registrations);
        }

        public CommanderModule([NotNull] Action<CommandRegistrationBuilder> register)
            : this(register, CommandLineMapper.DefaultConverter) { }

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
                .RegisterType<CommandExecutor>()
                .As<ICommandExecutor>();

            builder
                .RegisterGeneric(typeof(CommandServiceProvider<>));


            builder
                .RegisterModule(_registrations);

            builder
                .RegisterSource(new TypeListSource());
        }
    }
}