using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Data;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine.Commands
{
    public interface IConsoleCommand : ICommand
    {
        [NotNull]
        IImmutableNameSet Name { get; }        

        [NotNull]
        ParameterMetadata Parameter { get; }
    }

    public class ConsoleCommand : IConsoleCommand
    {
        public ConsoleCommand([NotNull] ICommand command, [NotNull] IImmutableNameSet name, [CanBeNull] Type parameterType)
        {
            Command = command;
            Name = name;
            Parameter = CommandParameterFactory.CreateCommandParameterMetadata(parameterType); ;
        }

        public event EventHandler CanExecuteChanged;

        private ICommand Command { get; }

        public IImmutableNameSet Name { get; }

        public ParameterMetadata Parameter { get; }

        public bool CanExecute(object parameter)
        {
            return Command.CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (parameter is ConsoleContext consoleContext)
            {
                parameter = CommandParameterFactory.CreateParameter(Parameter, consoleContext);
                Command.Execute(parameter);
            }
            else
            {
                Command.Execute(parameter);
            }
        }
    }
}
