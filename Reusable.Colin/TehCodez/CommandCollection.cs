using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Reusable.Colin.Collections;
using Reusable.Colin.Services;
using Reusable.TypeConversion;

namespace Reusable.Colin
{
    // This class isn't in the Collections namespace because it belongs to the main API and shold be easily reachable by including only the main namespace.
    public class CommandCollection : IImmutableDictionary<ImmutableNameSet, Services.CommandExecutor>
    {
        private readonly IImmutableDictionary<ImmutableNameSet, Services.CommandExecutor> _commands;

        public CommandCollection()
        {
            _commands = ImmutableDictionary.Create<ImmutableNameSet, Services.CommandExecutor>(ImmutableNameSet.Comparer);
        }

        internal CommandCollection(IImmutableDictionary<ImmutableNameSet, Services.CommandExecutor> commands)
        {
            _commands = commands;
        }       

        #region IImmutableDictionary

        public Services.CommandExecutor this[ImmutableNameSet key] => _commands[key];

        public int Count => _commands.Count;

        public IEnumerable<ImmutableNameSet> Keys => _commands.Keys;

        public IEnumerable<Services.CommandExecutor> Values => _commands.Values;

        public bool ContainsKey(ImmutableNameSet key) => _commands.ContainsKey(key);

        public bool TryGetValue(ImmutableNameSet key, out Services.CommandExecutor value) => _commands.TryGetValue(key, out value);

        public bool Contains(KeyValuePair<ImmutableNameSet, Services.CommandExecutor> pair) => _commands.Contains(pair);

        public bool TryGetKey(ImmutableNameSet equalKey, out ImmutableNameSet actualKey) => _commands.TryGetKey(equalKey, out actualKey);

        public IImmutableDictionary<ImmutableNameSet, Services.CommandExecutor> Clear() => new CommandCollection(_commands.Clear());

        public IImmutableDictionary<ImmutableNameSet, Services.CommandExecutor> Add(ImmutableNameSet key, Services.CommandExecutor value) => new CommandCollection(_commands.Add(key, value));

        public IImmutableDictionary<ImmutableNameSet, Services.CommandExecutor> AddRange(IEnumerable<KeyValuePair<ImmutableNameSet, Services.CommandExecutor>> pairs) => new CommandCollection(_commands.AddRange(pairs));

        public IImmutableDictionary<ImmutableNameSet, Services.CommandExecutor> SetItem(ImmutableNameSet key, Services.CommandExecutor value) => new CommandCollection(_commands.SetItem(key, value));

        public IImmutableDictionary<ImmutableNameSet, Services.CommandExecutor> SetItems(IEnumerable<KeyValuePair<ImmutableNameSet, Services.CommandExecutor>> items) => new CommandCollection(_commands.SetItems(items));

        public IImmutableDictionary<ImmutableNameSet, Services.CommandExecutor> RemoveRange(IEnumerable<ImmutableNameSet> keys) => new CommandCollection(_commands.RemoveRange(keys));

        public IImmutableDictionary<ImmutableNameSet, Services.CommandExecutor> Remove(ImmutableNameSet key) => new CommandCollection(_commands.Remove(key));

        public IEnumerator<KeyValuePair<ImmutableNameSet, Services.CommandExecutor>> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion        
    }
}
