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
    public class CommandCollection : IEnumerable<CommandInfo>
    {
        private readonly Dictionary<StringSetCI, CommandInfo> _commands = new Dictionary<StringSetCI, CommandInfo>();

        public CommandInfo this[StringSetCI nameSet] => _commands.TryGetValue(nameSet, out CommandInfo command) ? command : null;

        public CommandInfo this[string name] => this[StringSetCI.Create(name)];

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

        #endregion
    }    
}
