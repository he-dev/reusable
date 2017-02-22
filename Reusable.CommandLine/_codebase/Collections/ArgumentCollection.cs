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
            //set => _arguments[name] = value;
        }

        public void Add(ImmutableHashSet<string> name, string value)
        {
            var argument = this[name];
            if (argument == null) _arguments.Add(argument = new CommandLineArgument(name));
            if (!string.IsNullOrEmpty(value)) argument.Add(value);
        }

        #region IEnumerable

        public IEnumerator<IGrouping<ImmutableHashSet<string>, string>> GetEnumerator() => _arguments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}