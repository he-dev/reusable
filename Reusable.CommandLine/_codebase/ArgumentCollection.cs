using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartCommandLine
{
    public class ArgumentCollection : IEnumerable<Argument>
    {
        private readonly List<Argument> _arguments = new List<Argument>();

        public Argument this[params string[] names]
            => this[(IEnumerable<string>)names];

        public Argument this[IEnumerable<string> names]
            => _arguments.SingleOrDefault(x => names.Contains(x.Key, StringComparer.OrdinalIgnoreCase));

        public int Count => _arguments.Count;

        public Argument Anonymous 
            => this[string.Empty];

        public string CommandName
            => Anonymous?.FirstOrDefault();

        public bool TryGetValues(IEnumerable<string> names, int position, out IEnumerable<string> values)
        {
            var argument = this[names] ?? (position > 0 ? this[string.Empty] : null);

            if (argument == null)
            {
                values = Enumerable.Empty<string>();
                return false;
            }

            if (position > 0)
            {
                var value = argument.ElementAtOrDefault(position);
                if (string.IsNullOrEmpty(value))
                {
                    values = Enumerable.Empty<string>();
                    return false;
                }

                values = new[] { value };
                return true;
            }

            values = argument;
            return true;
        }

        internal Argument Add(string name, params string[] values)
        {
            var argument = this[name];

            if (argument == null)
            {
                _arguments.Add(argument = new Argument(name, values));
            }

            return argument;
        }

        public IEnumerator<Argument> GetEnumerator() => _arguments.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}