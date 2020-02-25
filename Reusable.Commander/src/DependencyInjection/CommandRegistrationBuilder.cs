using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Reusable.Commander.Commands;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Commander.DependencyInjection
{
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
            var command = new CommandInfo(typeof(TCommand)).Pipe(AddCommand);

            return
                Builder
                    .RegisterType<TCommand>()
                    .Named<ICommand>(command.RegistrationKey);
        }

        public IRegistrationBuilder<Lambda<TParameter>, SimpleActivatorData, SingleRegistrationStyle> Register<TParameter>(ArgumentName name, ExecuteDelegate<TParameter> execute) where TParameter : CommandParameter, new()
        {
            var command = new CommandInfo(typeof(Lambda<TParameter>), name).Pipe(AddCommand);
            return
                Builder
                    .Register(ctx => new Lambda<TParameter>(command.Name, execute))
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
            if (!Commands.Add(command))
            {
                throw DynamicException.Create("DuplicateCommandName", $"Command name '{command.Name.Primary}' is already in use.");
            }
        }

        IEnumerator<CommandInfo> IEnumerable<CommandInfo>.GetEnumerator() => Commands.GetEnumerator();

        public IEnumerator GetEnumerator() => ((IEnumerable)Commands).GetEnumerator();
    }
}