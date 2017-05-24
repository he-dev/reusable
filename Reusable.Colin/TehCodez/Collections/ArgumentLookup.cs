using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using JetBrains.Annotations;
using Reusable.Colin.Data;

namespace Reusable.Colin.Collections
{
    public class ArgumentLookup : ILookup<ImmutableNameSet, string>
    {
        private readonly IDictionary<ImmutableNameSet, CommandLineArgument> _arguments = new Dictionary<ImmutableNameSet, CommandLineArgument>(ImmutableNameSet.Comparer);

        internal ArgumentLookup() { }

        public IEnumerable<string> this[ImmutableNameSet name] => _arguments.TryGetValue(name, out CommandLineArgument argument) ? argument : Enumerable.Empty<string>();

        public int Count => _arguments.Count;

        public bool Contains(ImmutableNameSet name) => _arguments.ContainsKey(name);

        [ContractAnnotation("name: null => halt; value: null => halt")]
        public void Add([NotNull] ImmutableNameSet name, [NotNull] string value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (_arguments.TryGetValue(name, out CommandLineArgument argument))
            {
                argument.Add(value);
            }
            else
            {
                _arguments.Add(name, new CommandLineArgument(name) { value });
            }
        }

        [ContractAnnotation("name: null => halt")]
        public void Add([NotNull] ImmutableNameSet name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (_arguments.ContainsKey(name))
            {
                throw new InvalidOperationException($"An argument with the name '{name}' is already added.");
            }

            _arguments.Add(name, new CommandLineArgument(name));
        }

        public void Add(string name, string value) => Add(ImmutableNameSet.Create(name), value);

        #region IEnumerable

        public IEnumerator<IGrouping<ImmutableNameSet, string>> GetEnumerator() => _arguments.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}