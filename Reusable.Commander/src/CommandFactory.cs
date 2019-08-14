using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Autofac;
using Reusable.Exceptionize;

namespace Reusable.Commander
{
    public interface ICommandFactory
    {
        ICommand CreateCommand(NameSet commandName);
    }

    internal class CommandFactory : ICommandFactory
    {
        private readonly ILifetimeScope _scope;
        //private readonly IEnumerable<ICommand> _commands;

        // You used IIndex<,> here before but it didn't work with decorated commands.
        // You tested it in LINQPad with "Autofac and decorators".
        public CommandFactory(ILifetimeScope scope)
        {
            _scope = scope;
            //_commands = commands;
        }

        public ICommand CreateCommand(NameSet commandName)
        {
            var commands = _scope.Resolve<IEnumerable<ICommand>>();
            return commands.Where(cmd => cmd.Name == commandName).SingleOrThrow
            (
                onEmpty: () => DynamicException.Create($"CommandNotFound", $"Could not find command '{commandName.Default}'.")
            );
        }
    }
}