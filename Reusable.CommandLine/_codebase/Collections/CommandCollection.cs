using Reusable.Shelly.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Input;
using System.Collections.Immutable;

namespace Reusable.Shelly.Collections
{
    public class CommandCollection : IEnumerable<ICommand>
    {
        private readonly IImmutableList<ICommand> _commands;

        public CommandCollection(IEnumerable<ICommand> commands) => _commands.ToImmutableList();

        public ICommand this[ISet<string> nameSet] => _commands.SingleOrDefault(command => command.CanExecute(nameSet));

        public ICommand this[string name] => this[ImmutableNameSet.Create(name)];

        public bool TryGetCommand(ImmutableHashSet<string> nameSet, out ICommand command)
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
