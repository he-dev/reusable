using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Reusable.Colin.Collections;
using Reusable.Colin.Services;
using Reusable.TypeConversion;

namespace Reusable.Colin
{
    public class CommandLine : IImmutableDictionary<ImmutableNameSet, CommandExecutor>
    {
        private readonly IImmutableDictionary<ImmutableNameSet, CommandExecutor> _commands;

        public CommandLine()
        {
            _commands = ImmutableDictionary.Create<ImmutableNameSet, CommandExecutor>(ImmutableNameSet.Comparer);
        }

        internal CommandLine(IImmutableDictionary<ImmutableNameSet, CommandExecutor> commands)
        {
            _commands = commands;
        }

        public static readonly ImmutableNameSet DefaultCommandName = ImmutableNameSet.Create("Default");

        #region IImmutableDictionary

        public CommandExecutor this[ImmutableNameSet key] => _commands[key];

        public int Count => _commands.Count;

        public IEnumerable<ImmutableNameSet> Keys => _commands.Keys;

        public IEnumerable<CommandExecutor> Values => _commands.Values;

        public bool ContainsKey(ImmutableNameSet key) => _commands.ContainsKey(key);

        public bool TryGetValue(ImmutableNameSet key, out CommandExecutor value) => _commands.TryGetValue(key, out value);

        public bool Contains(KeyValuePair<ImmutableNameSet, CommandExecutor> pair) => _commands.Contains(pair);

        public bool TryGetKey(ImmutableNameSet equalKey, out ImmutableNameSet actualKey) => _commands.TryGetKey(equalKey, out actualKey);

        public IImmutableDictionary<ImmutableNameSet, CommandExecutor> Clear() => new CommandLine(_commands.Clear());

        public IImmutableDictionary<ImmutableNameSet, CommandExecutor> Add(ImmutableNameSet key, CommandExecutor value) => new CommandLine(_commands.Add(key, value));

        public IImmutableDictionary<ImmutableNameSet, CommandExecutor> AddRange(IEnumerable<KeyValuePair<ImmutableNameSet, CommandExecutor>> pairs) => new CommandLine(_commands.AddRange(pairs));

        public IImmutableDictionary<ImmutableNameSet, CommandExecutor> SetItem(ImmutableNameSet key, CommandExecutor value) => new CommandLine(_commands.SetItem(key, value));

        public IImmutableDictionary<ImmutableNameSet, CommandExecutor> SetItems(IEnumerable<KeyValuePair<ImmutableNameSet, CommandExecutor>> items) => new CommandLine(_commands.SetItems(items));

        public IImmutableDictionary<ImmutableNameSet, CommandExecutor> RemoveRange(IEnumerable<ImmutableNameSet> keys) => new CommandLine(_commands.RemoveRange(keys));

        public IImmutableDictionary<ImmutableNameSet, CommandExecutor> Remove(ImmutableNameSet key) => new CommandLine(_commands.Remove(key));

        public IEnumerator<KeyValuePair<ImmutableNameSet, CommandExecutor>> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        public static readonly TypeConverter DefaultConverter = TypeConverter.Empty.Add(
            new StringToSByteConverter(),
            new StringToByteConverter(),
            new StringToCharConverter(),
            new StringToInt16Converter(),
            new StringToInt32Converter(),
            new StringToInt64Converter(),
            new StringToUInt16Converter(),
            new StringToUInt32Converter(),
            new StringToUInt64Converter(),
            new StringToSingleConverter(),
            new StringToDoubleConverter(),
            new StringToDecimalConverter(),
            new StringToColorConverter(),
            new StringToBooleanConverter(),
            new StringToDateTimeConverter(),
            new StringToEnumConverter(),
            new EnumerableToArrayConverter());
    }
}
