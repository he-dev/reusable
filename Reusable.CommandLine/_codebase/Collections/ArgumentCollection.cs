using Reusable.Shelly.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Collections;

namespace Reusable.Shelly.Collections
{
    public class ArgumentCollection : IEnumerable<IGrouping<ImmutableHashSet<string>, string>>
    {
        private readonly IList<CommandLineArgument> _arguments = new List<CommandLineArgument>();

        internal ArgumentCollection() { }

        public ImmutableHashSet<string> CommandName
        {
            get
            {
                var values = _arguments.SingleOrDefault(g => g.Key.Overlaps(ImmutableNameSet.Create(string.Empty)));
                return ImmutableNameSet.Create(values.FirstOrDefault() ?? string.Empty);
            }
        }

        public CommandLineArgument this[ImmutableHashSet<string> name]
        {
            get => _arguments.SingleOrDefault(g => g.Key.Overlaps(name));
        }

        public CommandLineArgument this[params string[] names]
        {
            get => this[ImmutableNameSet.Create(names)];
        }

        public CommandLineArgument this[int position]
        {
            get
            {
                if (!(position > 0)) return null;
                var value = this[ImmutableNameSet.Empty].ElementAtOrDefault(position);
                return value == null ? null : new CommandLineArgument(ImmutableNameSet.Empty) { value };
            }
        }

        public void Add(ImmutableHashSet<string> name, string value)
        {
            var argument = this[name];
            if (argument == null) _arguments.Add(argument = new CommandLineArgument(name));
            if (!string.IsNullOrEmpty(value)) argument.Add(value);
        }

        public void Add(string name, string value) => Add(ImmutableNameSet.Create(name), value);

        #region IEnumerable

        public IEnumerator<IGrouping<ImmutableHashSet<string>, string>> GetEnumerator() => _arguments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}