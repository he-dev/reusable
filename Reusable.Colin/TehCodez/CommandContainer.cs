using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows.Input;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Commands;
using Reusable.CommandLine.Data;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine
{
    // This class isn't in the Collections namespace because it belongs to the main API and shold be easily reachable by including only the main namespace.
    public class CommandContainer : IImmutableDictionary<IImmutableNameSet, CommandMetadata>
    {
        private readonly IImmutableDictionary<IImmutableNameSet, CommandMetadata> _commands;

        private CommandContainer()
        {
            _commands = ImmutableDictionary.Create<IImmutableNameSet, CommandMetadata>();
        }

        internal CommandContainer(IImmutableDictionary<IImmutableNameSet, CommandMetadata> commands)
        {
            _commands = commands;
        }

        public static CommandContainer Empty => new CommandContainer();

        #region IImmutableDictionary

        public CommandMetadata this[IImmutableNameSet key] => _commands[key];

        public int Count => _commands.Count;

        public IEnumerable<IImmutableNameSet> Keys => _commands.Keys;

        public IEnumerable<CommandMetadata> Values => _commands.Values;

        public bool ContainsKey(IImmutableNameSet key) => _commands.ContainsKey(key);

        public bool TryGetValue(IImmutableNameSet key, out CommandMetadata value) => _commands.TryGetValue(key, out value);

        public bool Contains(KeyValuePair<IImmutableNameSet, CommandMetadata> pair) => _commands.Contains(pair);

        public bool TryGetKey(IImmutableNameSet equalKey, out IImmutableNameSet actualKey) => _commands.TryGetKey(equalKey, out actualKey);

        public IImmutableDictionary<IImmutableNameSet, CommandMetadata> Clear() => new CommandContainer(_commands.Clear());

        public IImmutableDictionary<IImmutableNameSet, CommandMetadata> Add(IImmutableNameSet key, CommandMetadata value) => new CommandContainer(_commands.Add(key, value));

        public IImmutableDictionary<IImmutableNameSet, CommandMetadata> AddRange(IEnumerable<KeyValuePair<IImmutableNameSet, CommandMetadata>> pairs) => new CommandContainer(_commands.AddRange(pairs));

        public IImmutableDictionary<IImmutableNameSet, CommandMetadata> SetItem(IImmutableNameSet key, CommandMetadata value) => new CommandContainer(_commands.SetItem(key, value));

        public IImmutableDictionary<IImmutableNameSet, CommandMetadata> SetItems(IEnumerable<KeyValuePair<IImmutableNameSet, CommandMetadata>> items) => new CommandContainer(_commands.SetItems(items));

        public IImmutableDictionary<IImmutableNameSet, CommandMetadata> RemoveRange(IEnumerable<IImmutableNameSet> keys) => new CommandContainer(_commands.RemoveRange(keys));

        public IImmutableDictionary<IImmutableNameSet, CommandMetadata> Remove(IImmutableNameSet key) => new CommandContainer(_commands.Remove(key));

        public IEnumerator<KeyValuePair<IImmutableNameSet, CommandMetadata>> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion        
    }
}
