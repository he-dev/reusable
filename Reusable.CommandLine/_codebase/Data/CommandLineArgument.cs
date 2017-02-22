using Reusable.Shelly.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Reusable.Shelly.Data
{
    public class CommandLineArgument : List<string>, IGrouping<ImmutableHashSet<string>, string>
    {
        internal CommandLineArgument(string key) => Key = ImmutableNameSet.Create(key);

        internal CommandLineArgument(ImmutableHashSet<string> key) => Key = key;

        public ImmutableHashSet<string> Key { get; }
    }
}