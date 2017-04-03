using Reusable.Colin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Input;
using System.Collections.Immutable;

namespace Reusable.Colin.Collections
{
    public class CommandCollection : IEnumerable<ICommand>
    {
        private readonly IImmutableList<ICommand> _commands;

        public CommandCollection(IEnumerable<ICommand> commands) => _commands = commands.ToImmutableList();

        public ICommand this[ImmutableNameSet nameSet] => _commands.SingleOrDefault(command => command.CanExecute(nameSet));

        public ICommand this[string name] => this[ImmutableNameSet.Create(name)];

        public bool TryGetCommand(ImmutableNameSet nameSet, out ICommand command)
        {
            command = this[nameSet];
            return command != null;
        }

        //public void Add(ICommand command)
        //{
        //    //if (_commands.Any)
        //    _commands.Add(command);
        //}

        #region IEnumerable

        public IEnumerator<ICommand> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
