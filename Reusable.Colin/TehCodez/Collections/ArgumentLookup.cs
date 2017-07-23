using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.CommandLine.Data;

namespace Reusable.CommandLine.Collections
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ArgumentLookup : ILookup<IImmutableNameSet, string>
    {
        private readonly IDictionary<IEquatable<IImmutableNameSet>, ArgumentGrouping> _arguments = new Dictionary<IEquatable<IImmutableNameSet>, ArgumentGrouping>();

        internal ArgumentLookup() { }

        private string DebuggerDisplay => this.ToCommandLineString("-:");

        public static ArgumentLookup Empty => new ArgumentLookup();

        #region ILookup

        public IEnumerable<string> this[IImmutableNameSet name] => _arguments.TryGetValue(name, out var argument) ? argument : Enumerable.Empty<string>();

        public int Count => _arguments.Count;

        public bool Contains(IImmutableNameSet name) => _arguments.ContainsKey(name);

        #endregion

        #region IEnumerable

        public IEnumerator<IGrouping<IImmutableNameSet, string>> GetEnumerator() => _arguments.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        [ContractAnnotation("name: null => halt")]
        public void Add([NotNull] IImmutableNameSet name)
        {
            if (name == null) { throw new ArgumentNullException(nameof(name)); }

            if (_arguments.ContainsKey(name))
            {
                throw new InvalidOperationException($"An argument with the name '{name}' is already added.");
            }

            _arguments.Add(name, new ArgumentGrouping(name));
        }

        [ContractAnnotation("name: null => halt; value: null => halt")]
        public void Add([NotNull] IImmutableNameSet name, [NotNull] string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (_arguments.TryGetValue(name, out ArgumentGrouping argument))
            {
                argument.Add(value);
            }
            else
            {
                _arguments.Add(name, new ArgumentGrouping(name) { value });
            }
        }

        public void Add(string name, string value) => Add(ImmutableNameSet.Create(name), value);

        public void Add(ArgumentGrouping argumentGrouping)
        {
            _arguments.Add(argumentGrouping, argumentGrouping);
        }
    }
}