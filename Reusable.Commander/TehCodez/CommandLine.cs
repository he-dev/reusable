using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.CommandLine;

namespace Reusable.Commander
{
    // foo -bar -baz qux
    public interface ICommandLine : ILookup<SoftKeySet, string>
    {
    }

    
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class CommandLine : ICommandLine
    {
        public static readonly int CommandIndex = 0;
        
        private readonly IDictionary<SoftKeySet, CommandArgument> _arguments = new PainlessDictionary<SoftKeySet, CommandArgument>();

        internal CommandLine()
        {
        }

        private string DebuggerDisplay => ToString();

        public static CommandLine Empty => new CommandLine();

        #region ILookup

        public IEnumerable<string> this[SoftKeySet name] => _arguments.TryGetValue(name, out var argument) ? argument : Enumerable.Empty<string>();

        public int Count => _arguments.Count;

        public bool Contains(SoftKeySet name) => _arguments.ContainsKey(name);

        #endregion

        #region IEnumerable

        public IEnumerator<IGrouping<SoftKeySet, string>> GetEnumerator() => _arguments.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        [ContractAnnotation("name: null => halt")]
        public void Add([NotNull] SoftKeySet name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            _arguments.Add(name, new CommandArgument(name));
        }

        [ContractAnnotation("name: null => halt; value: null => halt")]
        public void Add([NotNull] SoftKeySet name, [NotNull] string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (_arguments.TryGetValue(name, out var argument))
            {
                argument.Add(value);
            }
            else
            {
                _arguments.Add(name, new CommandArgument(name) {value});
            }
        }

        public override string ToString()
        {
            return string.Join(" ", this.Select(argument => argument.ToString()));
        }
        
        public static implicit operator string(CommandLine commandLine) => commandLine?.ToString();
    }
}