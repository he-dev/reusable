using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Commands;
using Reusable.CommandLine.Data;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine
{
    // This class isn't in the Collections namespace because it belongs to the main API and shold be easily reachable by including only the main namespace.
    public class CommandContainer : IImmutableDictionary<IImmutableNameSet, IConsoleCommand>
    {
        private readonly IImmutableDictionary<IImmutableNameSet, IConsoleCommand> _commands;

        private CommandContainer()
        {
            _commands = ImmutableDictionary.Create<IImmutableNameSet, IConsoleCommand>();
        }

        internal CommandContainer(IImmutableDictionary<IImmutableNameSet, IConsoleCommand> commands)
        {
            _commands = commands;
        }

        public static CommandContainer Empty => new CommandContainer();

        #region IImmutableDictionary

        public IConsoleCommand this[IImmutableNameSet key] => _commands[key];

        public int Count => _commands.Count;

        public IEnumerable<IImmutableNameSet> Keys => _commands.Keys;

        public IEnumerable<IConsoleCommand> Values => _commands.Values;

        public bool ContainsKey(IImmutableNameSet key) => _commands.ContainsKey(key);

        public bool TryGetValue(IImmutableNameSet key, out IConsoleCommand value) => _commands.TryGetValue(key, out value);

        public bool Contains(KeyValuePair<IImmutableNameSet, IConsoleCommand> pair) => _commands.Contains(pair);

        public bool TryGetKey(IImmutableNameSet equalKey, out IImmutableNameSet actualKey) => _commands.TryGetKey(equalKey, out actualKey);

        public IImmutableDictionary<IImmutableNameSet, IConsoleCommand> Clear() => new CommandContainer(_commands.Clear());

        public IImmutableDictionary<IImmutableNameSet, IConsoleCommand> Add(IImmutableNameSet key, IConsoleCommand value) => new CommandContainer(_commands.Add(key, value));

        public IImmutableDictionary<IImmutableNameSet, IConsoleCommand> AddRange(IEnumerable<KeyValuePair<IImmutableNameSet, IConsoleCommand>> pairs) => new CommandContainer(_commands.AddRange(pairs));

        public IImmutableDictionary<IImmutableNameSet, IConsoleCommand> SetItem(IImmutableNameSet key, IConsoleCommand value) => new CommandContainer(_commands.SetItem(key, value));

        public IImmutableDictionary<IImmutableNameSet, IConsoleCommand> SetItems(IEnumerable<KeyValuePair<IImmutableNameSet, IConsoleCommand>> items) => new CommandContainer(_commands.SetItems(items));

        public IImmutableDictionary<IImmutableNameSet, IConsoleCommand> RemoveRange(IEnumerable<IImmutableNameSet> keys) => new CommandContainer(_commands.RemoveRange(keys));

        public IImmutableDictionary<IImmutableNameSet, IConsoleCommand> Remove(IImmutableNameSet key) => new CommandContainer(_commands.Remove(key));

        public IEnumerator<KeyValuePair<IImmutableNameSet, IConsoleCommand>> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion        
    }

    public static class CommandContainerExtensions
    {
        [CanBeNull]
        public static IConsoleCommand Find([NotNull] this CommandContainer commands, [CanBeNull] IImmutableNameSet commandName)
        {
            // The help-command requires special treatment and does not count as a "real" command so exclude it from count.

            var nonHelpCommands = commands.ToLookup(c => !c.Value.Name.Equals(ImmutableNameSet.Help));

            return
                nonHelpCommands[true].Count() == 1
                    ? nonHelpCommands[true].Single().Value
                    : (commandName != null && commands.TryGetValue(commandName, out var command) ? command : default(IConsoleCommand));
        }
    }
}
