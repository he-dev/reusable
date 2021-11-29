using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Autofac;
using Reusable.Commander.Utilities;

namespace Reusable.Commander
{
    public interface ICommandFactory
    {
        ICommand CreateCommand(string commandName);
    }

    internal class CommandFactory : ICommandFactory
    {
        private readonly ILifetimeScope _scope;

        public CommandFactory(ILifetimeScope scope) => _scope = scope;

        public ICommand CreateCommand(string commandName)
        {
            var commandTypes =
                from c in _scope.Resolve<IEnumerable<ICommand>>()
                where c.Name.Equals(commandName)
                select c;
            return commandTypes.SingleOrThrow(onEmpty: ("CommandNotFound", $"Could not find command '{commandName}'."));
        }
    }
}