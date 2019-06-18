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
    public interface ICommandLine : IEnumerable<CommandParameter>
    {
        [CanBeNull]
        CommandParameter this[Identifier id] { get; }
    }

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class CommandLine : ICommandLine
    {
        private readonly IDictionary<Identifier, CommandParameter> _parameters = new Dictionary<Identifier, CommandParameter>();

        internal CommandLine() { }

        private string DebuggerDisplay => ToString();

        public CommandParameter this[Identifier id] => _parameters.TryGetValue(id, out var argument) ? argument : default;

        #region IEnumerable

        public IEnumerator<CommandParameter> GetEnumerator() => _parameters.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        [ContractAnnotation("id: null => halt")]
        public void Add([NotNull] Identifier id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            _parameters.Add(id, new CommandParameter(id));
        }

        [ContractAnnotation("id: null => halt; value: null => halt")]
        public void Add([NotNull] Identifier id, [NotNull] string value)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (_parameters.TryGetValue(id, out var argument))
            {
                argument.Add(value);
            }
            else
            {
                _parameters.Add(id, new CommandParameter(id) { value });
            }
        }

        [ContractAnnotation("name: null => halt")]
        public void Add([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            Add(Identifier.FromName(name));
        }

        [ContractAnnotation("name: null => halt; value: null => halt")]
        public void Add([NotNull] string name, [NotNull] string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));

            Add(Identifier.FromName(name), value);
        }

        public override string ToString() => string.Join(" ", this.Select(argument => argument.ToString()));

        public static implicit operator string(CommandLine commandLine) => commandLine?.ToString();
    }
}