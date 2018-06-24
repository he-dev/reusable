using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Commander
{
    // foo -bar -baz qux
    public interface ICommandLine : ILookup<SoftKeySet, string> { }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class CommandLine : ICommandLine
    {
        public static readonly int CommandIndex = 0;

        private readonly IDictionary<SoftKeySet, CommandArgument> _arguments = new Dictionary<SoftKeySet, CommandArgument>();

        internal CommandLine() { }

        private string DebuggerDisplay => ToString();

        public static CommandLine Empty => new CommandLine();

        #region ILookup

        public IEnumerable<string> this[SoftKeySet name] => _arguments.TryGetValue(name, out var argument) ? argument : CommandArgument.Undefined;

        public int Count => _arguments.Count;

        public bool Contains(SoftKeySet name) => _arguments.ContainsKey(name);

        #endregion

        #region IEnumerable

        public IEnumerator<IGrouping<SoftKeySet, string>> GetEnumerator() => _arguments.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion       

        [ContractAnnotation("keySet: null => halt")]
        public void Add([NotNull] SoftKeySet keySet)
        {
            if (keySet == null) throw new ArgumentNullException(nameof(keySet));

            _arguments.Add(keySet, new CommandArgument(keySet));
        }

        [ContractAnnotation("keySet: null => halt; value: null => halt")]
        public void Add([NotNull] SoftKeySet keySet, [NotNull] string value)
        {
            if (keySet == null) throw new ArgumentNullException(nameof(keySet));
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (_arguments.TryGetValue(keySet, out var argument))
            {
                argument.Add(value);
            }
            else
            {
                _arguments.Add(keySet, new CommandArgument(keySet) { value });
            }
        }

        [ContractAnnotation("key: null => halt")]
        public void Add([NotNull] SoftString key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            Add((SoftKeySet)key);
        }

        [ContractAnnotation("key: null => halt; value: null => halt")]
        public void Add([NotNull] SoftString key, [NotNull] string value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            Add((SoftKeySet)key, value);
        }

        public override string ToString() => string.Join(" ", this.Select(argument => argument.ToString()));

        public static implicit operator string(CommandLine commandLine) => commandLine?.ToString();
    }
}