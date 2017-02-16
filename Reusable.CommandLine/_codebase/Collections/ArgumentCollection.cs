using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Shelly.Collections
{
    public class ArgumentCollection
    {
        private readonly Dictionary<string, IList<string>> _arguments = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

        internal ArgumentCollection(IEnumerable<Argument> arguments)
        {
            _arguments = arguments.ToDictionary(x => x.Key, x => (IList<string>)x, StringComparer.OrdinalIgnoreCase);
        }

        public string CommandName => _arguments[string.Empty].FirstOrDefault();

        public IList<string> this[string name]
        {
            get => _arguments.TryGetValue(name, out IList<string> values) ? values : null;
            set => _arguments[name] = value;
        }

        public bool ContainsArgument(string argumentName) => _arguments.ContainsKey(argumentName);
    }
}