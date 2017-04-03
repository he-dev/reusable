using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Collections;
using Reusable.Colin.Data;

namespace Reusable.Colin.Collections
{
    public class ArgumentCollection : IEnumerable<IGrouping<ImmutableNameSet, string>>
    {
        private readonly IList<CommandLineArgument> _arguments = new List<CommandLineArgument>();

        internal ArgumentCollection() { }

        public CommandLineArgument AnonymousArguments
        {
            get => _arguments.SingleOrDefault(g => g.Key.Overlaps(ImmutableNameSet.Empty));
        }

        public ImmutableNameSet CommandName
        {
            // Command-name is at argument-0.
            get
            {
                var commandName = AnonymousArguments?.FirstOrDefault();
                return string.IsNullOrEmpty(commandName) ? null : ImmutableNameSet.Create(commandName);
            }
        }

        public CommandLineArgument this[ImmutableNameSet name]
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

        public void Add(ImmutableNameSet name, string value)
        {
            var argument = this[name];
            if (argument == null) _arguments.Add(argument = new CommandLineArgument(name));
            if (!string.IsNullOrEmpty(value)) argument.Add(value);
        }

        public void Add(string name, string value) => Add(ImmutableNameSet.Create(name), value);

        #region IEnumerable

        public IEnumerator<IGrouping<ImmutableNameSet, string>> GetEnumerator() => _arguments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}