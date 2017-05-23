using System.Collections.Generic;
using System.Linq;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Data
{
    public class CommandLineArgument : List<string>, IGrouping<ImmutableNameSet, string>
    {
        internal CommandLineArgument(ImmutableNameSet key) => Key = key;

        public ImmutableNameSet Key { get; }
    }
}