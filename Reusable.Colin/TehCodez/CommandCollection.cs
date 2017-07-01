using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine
{
    // This class isn't in the Collections namespace because it belongs to the main API and shold be easily reachable by including only the main namespace.
    public class CommandCollection : IImmutableDictionary<IImmutableNameSet, CommandMapping>
    {
        private readonly IImmutableDictionary<IImmutableNameSet, CommandMapping> _commands;

        private CommandCollection()
        {
            _commands = ImmutableDictionary.Create<IImmutableNameSet, CommandMapping>(ImmutableNameSet.Comparer);
        }

        internal CommandCollection(IImmutableDictionary<IImmutableNameSet, CommandMapping> commands)
        {
            _commands = commands;
        }       

        public static CommandCollection Empty => new CommandCollection();

        #region IImmutableDictionary

        public CommandMapping this[IImmutableNameSet key] => _commands[key];

        public int Count => _commands.Count;

        public IEnumerable<IImmutableNameSet> Keys => _commands.Keys;

        public IEnumerable<CommandMapping> Values => _commands.Values;

        public bool ContainsKey(IImmutableNameSet key) => _commands.ContainsKey(key);

        public bool TryGetValue(IImmutableNameSet key, out CommandMapping value) => _commands.TryGetValue(key, out value);

        public bool Contains(KeyValuePair<IImmutableNameSet, CommandMapping> pair) => _commands.Contains(pair);

        public bool TryGetKey(IImmutableNameSet equalKey, out IImmutableNameSet actualKey) => _commands.TryGetKey(equalKey, out actualKey);

        public IImmutableDictionary<IImmutableNameSet, CommandMapping> Clear() => new CommandCollection(_commands.Clear());

        public IImmutableDictionary<IImmutableNameSet, CommandMapping> Add(IImmutableNameSet key, CommandMapping value) => new CommandCollection(_commands.Add(key, value));

        public IImmutableDictionary<IImmutableNameSet, CommandMapping> AddRange(IEnumerable<KeyValuePair<IImmutableNameSet, CommandMapping>> pairs) => new CommandCollection(_commands.AddRange(pairs));

        public IImmutableDictionary<IImmutableNameSet, CommandMapping> SetItem(IImmutableNameSet key, CommandMapping value) => new CommandCollection(_commands.SetItem(key, value));

        public IImmutableDictionary<IImmutableNameSet, CommandMapping> SetItems(IEnumerable<KeyValuePair<IImmutableNameSet, CommandMapping>> items) => new CommandCollection(_commands.SetItems(items));

        public IImmutableDictionary<IImmutableNameSet, CommandMapping> RemoveRange(IEnumerable<IImmutableNameSet> keys) => new CommandCollection(_commands.RemoveRange(keys));

        public IImmutableDictionary<IImmutableNameSet, CommandMapping> Remove(IImmutableNameSet key) => new CommandCollection(_commands.Remove(key));

        public IEnumerator<KeyValuePair<IImmutableNameSet, CommandMapping>> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion        
    }
}
