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
    public class CommandCollection : IEnumerable<CommandInfo>, ICollection
    {
        private readonly Dictionary<StringSet, CommandInfo> _commands = new Dictionary<StringSet, CommandInfo>(new HashSetOverlapsComparer<string>());


        public CommandInfo this[StringSet nameSet] => _commands.TryGetValue(nameSet, out CommandInfo command) ? command : null;

        public CommandInfo this[string name] => this[StringSet.CreateCI(name)];

        public bool TryGetCommand(StringSet nameSet, out CommandInfo command) => _commands.TryGetValue(nameSet, out command);

        public void Add(CommandInfo command)
        {
            CommandValidator.ValidatePropertyNames(command.GetType());
            _commands.Add(command.Names, command);
        }

        //public void Add(ICommand command, StringSetCI names, CommandParameterCollection parameters)
        //{
        //    //if (_commands.ContainsKey(StringSetCI.Create(names))) throw new ArgumentException($"Command '{command.GetType().FullName}' cannot be added because there is already another command with this name: \"{string.Join(", ", CommandReflection.GetCommandNames(command.GetType()))}\".");
        //    Add(new CommandInfo(command, names, parameters));
        //}

        //public void Add(ICommand command, CommandParameterCollection parameters)
        //{
        //    //if (_commands.ContainsKey(StringSetCI.Create(names))) throw new ArgumentException($"Command '{command.GetType().FullName}' cannot be added because there is already another command with this name: \"{string.Join(", ", CommandReflection.GetCommandNames(command.GetType()))}\".");
        //    Add(new CommandInfo(command, parameters));
        //}

        //public void Add(ICommand command, Type parameterType) => Add(command, new CommandParameterCollection(parameterType));

        #region IEnumerable

        public IEnumerator<CommandInfo> GetEnumerator() => _commands.Values.GetEnumerator();

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
