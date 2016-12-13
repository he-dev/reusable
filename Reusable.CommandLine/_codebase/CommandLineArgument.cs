using System.Collections.Generic;
using System.Linq;

namespace Reusable.Shelly
{
    internal class CommandLineArgument : List<string>, IGrouping<string, string>
    {
        internal CommandLineArgument(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}