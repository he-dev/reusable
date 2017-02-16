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
    public class CommandCollection : IEnumerable<ICommand>
    {
        private readonly HashSet<ICommand> _commands = new HashSet<ICommand>(new CommandTypeComparer());

        public void Add(ICommand command)
        {
            CommandValidator.ValidatePropertyNames(command.GetType());
            if (!_commands.Add(command)) throw new ArgumentException($"Command '{command.GetType().FullName}' cannot be added because there is already another command with this name: \"{string.Join(", ", CommandReflection.GetCommandNames(command.GetType()))}\".");
        }

        public ICommand Find(string name) => _commands.FirstOrDefault(c => CommandReflection.GetCommandNames(c.GetType()).Contains(name, StringComparer.OrdinalIgnoreCase));

        public IEnumerator<ICommand> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class CommandTypeComparer : IEqualityComparer<ICommand>
    {
        public bool Equals(ICommand x, ICommand y) => CommandReflection.GetCommandNames(x.GetType()).SequenceEqual(CommandReflection.GetCommandNames(x.GetType()));
        public int GetHashCode(ICommand obj) => string.Join(", ", CommandReflection.GetCommandNames(obj.GetType())).GetHashCode();
    }
}
