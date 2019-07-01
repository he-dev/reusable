using System.Collections.Generic;
using System.Linq.Custom;
using Reusable.Exceptionize;

namespace Reusable.Commander
{
    public interface ICommandFactory
    {
        ICommand CreateCommand(NameSet commandName);
    }

    internal class CommandFactory : ICommandFactory
    {
        private readonly IEnumerable<ICommand> _commands;

        // You used IIndex<,> here before but it didn't work with decorated commands.
        // You tested it in LINQPad with "Autofac and decorators".
        public CommandFactory(IEnumerable<ICommand> commands)
        {
            _commands = commands;
        }

        public ICommand CreateCommand(NameSet commandName)
        {
            return _commands.SingleOrThrow
            (
                predicate: cmd => cmd.Name == commandName,
                onEmpty: () => DynamicException.Create($"CommandNotFound", $"Could not find command '{commandName.Default}'.")
            );
        }
    }
}