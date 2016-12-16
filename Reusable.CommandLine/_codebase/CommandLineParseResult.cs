using System.Collections.Generic;
using System.Linq;

namespace Reusable.Shelly
{
    public class CommandLineParseResult
    {
        internal CommandLineParseResult(string commandName, IEnumerable<IGrouping<string, string>> arguments)
        {
            CommandName = commandName;
            Arguments = arguments;
        }

        public string CommandName { get; }

        public IEnumerable<IGrouping<string, string>> Arguments { get; }
    }
}