using System.Collections.Generic;
using System.Linq;

namespace Reusable.Shelly
{
    internal class Argument : List<string>, IGrouping<string, string>
    {
        internal Argument(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }
}