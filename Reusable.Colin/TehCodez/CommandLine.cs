using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;
using Reusable.Colin.Collections;
using Reusable.Colin.Commands;

namespace Reusable.Colin
{
    public class CommandLine : IImmutableDictionary<ImmutableNameSet, ICommand>
    {
        private readonly IImmutableDictionary<ImmutableNameSet, ICommand> _commands;

        private CommandLine(CommandLine commandLine, IImmutableDictionary<ImmutableNameSet, ICommand> commands)
            : this(
                  commandLine.ArgumentPrefix,
                  commandLine.ArgumentValueSeparator,
                  commandLine.Log,
                  commands)
        { }

        internal CommandLine(char argumentPrefix, char argumentValueSeparator, Action<string> log, IImmutableDictionary<ImmutableNameSet, ICommand> commands)
        {
            ArgumentPrefix = argumentPrefix;
            ArgumentValueSeparator = argumentValueSeparator;
            Log = log;
            _commands = commands.ToImmutableDictionary(ImmutableNameSet.Comparer);
        }

        public static CommandLineBuilder Builder => new CommandLineBuilder();

        public char ArgumentPrefix { get; }

        public char ArgumentValueSeparator { get; }

        private Action<string> Log { get; }

        #region IImmutableDictionary

        public ICommand this[ImmutableNameSet key]
        {
            get { throw new NotImplementedException(); }
        }

        public int Count => _commands.Count;

        public IEnumerable<ImmutableNameSet> Keys => _commands.Keys;

        public IEnumerable<ICommand> Values => _commands.Values;

        public bool ContainsKey(ImmutableNameSet key) => _commands.ContainsKey(key);

        public bool TryGetValue(ImmutableNameSet key, out ICommand value) => _commands.TryGetValue(key, out value);

        public bool Contains(KeyValuePair<ImmutableNameSet, ICommand> pair) => _commands.Contains(pair);

        public bool TryGetKey(ImmutableNameSet equalKey, out ImmutableNameSet actualKey) => _commands.TryGetKey(equalKey, out actualKey);

        public IImmutableDictionary<ImmutableNameSet, ICommand> Clear() => new CommandLine(this, _commands.Clear());

        public IImmutableDictionary<ImmutableNameSet, ICommand> Add(ImmutableNameSet key, ICommand value) => new CommandLine(this, _commands.Add(key, value));

        public IImmutableDictionary<ImmutableNameSet, ICommand> AddRange(IEnumerable<KeyValuePair<ImmutableNameSet, ICommand>> pairs) => new CommandLine(this, _commands.AddRange(pairs));

        public IImmutableDictionary<ImmutableNameSet, ICommand> SetItem(ImmutableNameSet key, ICommand value) => new CommandLine(this, _commands.SetItem(key, value));

        public IImmutableDictionary<ImmutableNameSet, ICommand> SetItems(IEnumerable<KeyValuePair<ImmutableNameSet, ICommand>> items) => new CommandLine(this, _commands.SetItems(items));

        public IImmutableDictionary<ImmutableNameSet, ICommand> RemoveRange(IEnumerable<ImmutableNameSet> keys) => new CommandLine(this, _commands.RemoveRange(keys));

        public IImmutableDictionary<ImmutableNameSet, ICommand> Remove(ImmutableNameSet key) => new CommandLine(this, _commands.Remove(key));

        public IEnumerator<KeyValuePair<ImmutableNameSet, ICommand>> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
