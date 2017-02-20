using Reusable.Shelly.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Input;

namespace Reusable.Shelly.Collections
{
    public class CommandCollection : IEnumerable<ICommand>, ICollection
    {
        private readonly IList<ICommand> _commands = new List<ICommand>();

        public ICommand this[ISet<string> nameSet] => _commands.SingleOrDefault(command => command.CanExecute(nameSet));

        public ICommand this[string name] => this[ImmutableNameSet.Create(name)];

        public bool TryGetCommand(StringSet nameSet, out ICommand command)
        {
            command = this[nameSet];
            return command != null;
        }

        public void Add(ICommand command) => _commands.Add(command);

        #region IEnumerable

        public IEnumerator<ICommand> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollection

        public int Count => _commands.Count;

        public object SyncRoot => null;

        public bool IsSynchronized => false;

        #endregion
    }
}
