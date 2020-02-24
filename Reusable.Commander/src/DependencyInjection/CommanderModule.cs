using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using JetBrains.Annotations;
using Reusable.Commander.Commands;
using Reusable.Commander.Utilities;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Commander.DependencyInjection
{
    [PublicAPI]
    public class CommanderModule : Autofac.Module
    {
        private readonly Action<ICommandRegistrationBuilder> _build;

        public CommanderModule(Action<ICommandRegistrationBuilder> build) => _build = build;

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

            var crb = new CommandRegistrationBuilder { Builder = builder }.Pipe(_build);

            builder
                .RegisterInstance(crb.ToList());

            // builder
            //     .RegisterSource(new TypeListSource());
        }
    }

    public interface ICommandRegistrationBuilder : IEnumerable<CommandInfo>
    {
        IRegistrationBuilder<TCommand, ConcreteReflectionActivatorData, SingleRegistrationStyle> Register<TCommand>() where TCommand : ICommand;
        IRegistrationBuilder<Lambda<TParameter>, SimpleActivatorData, SingleRegistrationStyle> Register<TParameter>(ArgumentName name, ExecuteDelegate<TParameter> execute) where TParameter : CommandParameter, new();
        IRegistrationBuilder<Lambda<TParameter>, SimpleActivatorData, SingleRegistrationStyle> Register<TParameter>(ArgumentName name, Action<TParameter> execute) where TParameter : CommandParameter, new();
    }

    public class CommandRegistrationBuilder : ICommandRegistrationBuilder
    {
        internal ContainerBuilder Builder { get; set; }

        private HashSet<CommandInfo> Commands { get; } = new HashSet<CommandInfo>();

        public ICommandNameResolver CommandNameResolver { get; set; }

        public IRegistrationBuilder<TCommand, ConcreteReflectionActivatorData, SingleRegistrationStyle> Register<TCommand>() where TCommand : ICommand
        {
            var command =
                CommandNameResolver
                    .ResolveCommandName<TCommand>()
                    .Map(names => new CommandInfo(names, typeof(TCommand)))
                    .Pipe(AddCommand);

            return
                Builder
                    .RegisterType<TCommand>()
                    .Named<ICommand>(command.RegistrationKey);
        }

        public IRegistrationBuilder<Lambda<TParameter>, SimpleActivatorData, SingleRegistrationStyle> Register<TParameter>(ArgumentName name, ExecuteDelegate<TParameter> execute) where TParameter : CommandParameter, new()
        {
            var command = new CommandInfo(name, typeof(Lambda<TParameter>)).Pipe(AddCommand);
            return
                Builder
                    .Register(ctx => new Lambda<TParameter>(ctx.Resolve<ILogger<Lambda<TParameter>>>(), command.Name, execute))
                    .Named<ICommand>(command.RegistrationKey);
        }

        public IRegistrationBuilder<Lambda<TParameter>, SimpleActivatorData, SingleRegistrationStyle> Register<TParameter>(ArgumentName name, Action<TParameter> execute) where TParameter : CommandParameter, new()
        {
            return Register<TParameter>(name, (n, p, t) =>
            {
                execute(p);
                return Task.CompletedTask;
            });
        }

        private void AddCommand(CommandInfo command)
        {
            foreach (var cmd in Commands.Where(cmd => !Commands.Add(cmd)))
            {
                throw DynamicException.Create("DuplicateCommandName", $"Command name '{cmd.Name.First()}' is already in use.");
            }
        }

        IEnumerator<CommandInfo> IEnumerable<CommandInfo>.GetEnumerator() => Commands.GetEnumerator();

        public IEnumerator GetEnumerator() => ((IEnumerable)Commands).GetEnumerator();
    }
}