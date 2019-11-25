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

        // You used IIndex<,> here before but it didn't work with decorated commands.
        // You tested it in LINQPad with "Autofac and decorators".
        public CommandFactory(ILifetimeScope scope) => _scope = scope;

        public ICommand CreateCommand(string commandName)
        {
            var commandTypes =
                from t in _scope.Resolve<TypeList<ICommand>>()
                let name = t.GetMultiName()
                where name.Equals(commandName)
                select t;
            var commandType = commandTypes.SingleOrThrow(onEmpty: ("CommandNotFound", $"Could not find command '{commandName}'."));
            return _scope.Resolve(commandType) as ICommand;
        }
    }
}