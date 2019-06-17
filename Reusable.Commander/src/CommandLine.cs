using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Diagnostics;

namespace Reusable.Commander
{
    // foo -bar -baz qux
    public interface ICommandLine : ILookup<Identifier, string> { }

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class CommandLine : ICommandLine
    {
        private readonly IDictionary<Identifier, CommandArgument> _arguments = new Dictionary<Identifier, CommandArgument>();

        internal CommandLine() { }

        private string DebuggerDisplay => ToString();

        #region ILookup

        public IEnumerable<string> this[Identifier id] => _arguments.TryGetValue(id, out var argument) ? argument : CommandArgument.Empty;

        public int Count => _arguments.Count;

        public bool Contains(Identifier id) => _arguments.ContainsKey(id);

        #endregion

        #region IEnumerable

        public IEnumerator<IGrouping<Identifier, string>> GetEnumerator() => _arguments.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        [ContractAnnotation("id: null => halt")]
        public void Add([NotNull] Identifier id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            _arguments.Add(id, new CommandArgument(id));
        }

        [ContractAnnotation("id: null => halt; value: null => halt")]
        public void Add([NotNull] Identifier id, [NotNull] string value)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (_arguments.TryGetValue(id, out var argument))
            {
                argument.Add(value);
            }
            else
            {
                _arguments.Add(id, new CommandArgument(id) { value });
            }
        }

        [ContractAnnotation("id: null => halt")]
        public void Add([NotNull] SoftString id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            Add((Identifier)id);
        }

        [ContractAnnotation("id: null => halt; value: null => halt")]
        public void Add([NotNull] SoftString id, [NotNull] string value)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (value == null) throw new ArgumentNullException(nameof(value));

            Add((Identifier)id, value);
        }

        public override string ToString() => string.Join(" ", this.Select(argument => argument.ToString()));

        public static implicit operator string(CommandLine commandLine) => commandLine?.ToString();
    }
}