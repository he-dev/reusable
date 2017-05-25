using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Reusable.Colin.Collections;
using Reusable.Colin.Services;
using Reusable.TypeConversion;

namespace Reusable.Colin
{
    // This class isn't in the Collections namespace because it belongs to the main API and shold be easily reachable by including only the main namespace.
    public class CommandCollection : IImmutableDictionary<ImmutableNameSet, CommandMapping>
    {
        private readonly IImmutableDictionary<ImmutableNameSet, CommandMapping> _commands;

        private CommandCollection()
        {
            _commands = ImmutableDictionary.Create<ImmutableNameSet, CommandMapping>(ImmutableNameSet.Comparer);
        }

        internal CommandCollection(IImmutableDictionary<ImmutableNameSet, CommandMapping> commands)
        {
            _commands = commands;
        }       

        public static CommandCollection Empty => new CommandCollection();

        #region IImmutableDictionary

        public CommandMapping this[ImmutableNameSet key] => _commands[key];

        public int Count => _commands.Count;

        public IEnumerable<ImmutableNameSet> Keys => _commands.Keys;

        public IEnumerable<CommandMapping> Values => _commands.Values;

        public bool ContainsKey(ImmutableNameSet key) => _commands.ContainsKey(key);

        public bool TryGetValue(ImmutableNameSet key, out CommandMapping value) => _commands.TryGetValue(key, out value);

        public bool Contains(KeyValuePair<ImmutableNameSet, CommandMapping> pair) => _commands.Contains(pair);

        public bool TryGetKey(ImmutableNameSet equalKey, out ImmutableNameSet actualKey) => _commands.TryGetKey(equalKey, out actualKey);

        public IImmutableDictionary<ImmutableNameSet, CommandMapping> Clear() => new CommandCollection(_commands.Clear());

        public IImmutableDictionary<ImmutableNameSet, CommandMapping> Add(ImmutableNameSet key, CommandMapping value) => new CommandCollection(_commands.Add(key, value));

        public IImmutableDictionary<ImmutableNameSet, CommandMapping> AddRange(IEnumerable<KeyValuePair<ImmutableNameSet, CommandMapping>> pairs) => new CommandCollection(_commands.AddRange(pairs));

        public IImmutableDictionary<ImmutableNameSet, CommandMapping> SetItem(ImmutableNameSet key, CommandMapping value) => new CommandCollection(_commands.SetItem(key, value));

        public IImmutableDictionary<ImmutableNameSet, CommandMapping> SetItems(IEnumerable<KeyValuePair<ImmutableNameSet, CommandMapping>> items) => new CommandCollection(_commands.SetItems(items));

        public IImmutableDictionary<ImmutableNameSet, CommandMapping> RemoveRange(IEnumerable<ImmutableNameSet> keys) => new CommandCollection(_commands.RemoveRange(keys));

        public IImmutableDictionary<ImmutableNameSet, CommandMapping> Remove(ImmutableNameSet key) => new CommandCollection(_commands.Remove(key));

        public IEnumerator<KeyValuePair<ImmutableNameSet, CommandMapping>> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion        
    }
}
