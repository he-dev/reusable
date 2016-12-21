using System;
using System.Collections.Generic;

namespace Reusable.Shelly.Collections
{
    public class ArgumentCollection
    {
        private readonly Dictionary<string, List<string>> _arguments;

        internal ArgumentCollection(string commandName, Dictionary<string, List<string>> arguments)
        {
            CommandName = commandName;
            _arguments = arguments;
        }

        public string CommandName { get; }

        public List<string> this[string argumentName]
        {
            get
            {
                var argumentValues = (List<string>)null;
                return _arguments.TryGetValue(argumentName, out argumentValues) ? argumentValues : null;
            }
            set { _arguments[argumentName] = value; }
        }

        public bool ContainsArgument(string argumentName) => _arguments.ContainsKey(argumentName);
    }
}