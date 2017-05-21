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
    public class CommandLine : IImmutableDictionary<ImmutableNameSet, CommandInvoker>
    {
        private readonly IImmutableDictionary<ImmutableNameSet, CommandInvoker> _commands;

        private CommandLine(CommandLine commandLine, IImmutableDictionary<ImmutableNameSet, CommandInvoker> commands)
            : this(
                  commandLine.ArgumentPrefix,
                  commandLine.ArgumentValueSeparator,
                  commandLine.Log,
                  commands)
        { }

        internal CommandLine(char argumentPrefix, char argumentValueSeparator, Action<string> log, IImmutableDictionary<ImmutableNameSet, CommandInvoker> commands)
        {
            ArgumentPrefix = argumentPrefix;
            ArgumentValueSeparator = argumentValueSeparator;
            Log = log;
            _commands = commands.ToImmutableDictionary(ImmutableNameSet.Comparer);
        }

        public static readonly ImmutableNameSet DefaultCommandName = ImmutableNameSet.Create("Default");

        public static CommandLineBuilder Builder => new CommandLineBuilder();

        public char ArgumentPrefix { get; }

        public char ArgumentValueSeparator { get; }

        private Action<string> Log { get; }

        #region IImmutableDictionary

        public CommandInvoker this[ImmutableNameSet key]
        {
            get { throw new NotImplementedException(); }
        }

        public int Count => _commands.Count;

        public IEnumerable<ImmutableNameSet> Keys => _commands.Keys;

        public IEnumerable<CommandInvoker> Values => _commands.Values;

        public bool ContainsKey(ImmutableNameSet key) => _commands.ContainsKey(key);

        public bool TryGetValue(ImmutableNameSet key, out CommandInvoker value) => _commands.TryGetValue(key, out value);

        public bool Contains(KeyValuePair<ImmutableNameSet, CommandInvoker> pair) => _commands.Contains(pair);

        public bool TryGetKey(ImmutableNameSet equalKey, out ImmutableNameSet actualKey) => _commands.TryGetKey(equalKey, out actualKey);

        public IImmutableDictionary<ImmutableNameSet, CommandInvoker> Clear() => new CommandLine(this, _commands.Clear());

        public IImmutableDictionary<ImmutableNameSet, CommandInvoker> Add(ImmutableNameSet key, CommandInvoker value) => new CommandLine(this, _commands.Add(key, value));

        public IImmutableDictionary<ImmutableNameSet, CommandInvoker> AddRange(IEnumerable<KeyValuePair<ImmutableNameSet, CommandInvoker>> pairs) => new CommandLine(this, _commands.AddRange(pairs));

        public IImmutableDictionary<ImmutableNameSet, CommandInvoker> SetItem(ImmutableNameSet key, CommandInvoker value) => new CommandLine(this, _commands.SetItem(key, value));

        public IImmutableDictionary<ImmutableNameSet, CommandInvoker> SetItems(IEnumerable<KeyValuePair<ImmutableNameSet, CommandInvoker>> items) => new CommandLine(this, _commands.SetItems(items));

        public IImmutableDictionary<ImmutableNameSet, CommandInvoker> RemoveRange(IEnumerable<ImmutableNameSet> keys) => new CommandLine(this, _commands.RemoveRange(keys));

        public IImmutableDictionary<ImmutableNameSet, CommandInvoker> Remove(ImmutableNameSet key) => new CommandLine(this, _commands.Remove(key));

        public IEnumerator<KeyValuePair<ImmutableNameSet, CommandInvoker>> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
