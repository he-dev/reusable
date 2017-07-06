using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.CommandLine.Collections
{
    /// <summary>
    /// This class represents a single command-line argument with all its values.
    /// </summary>
    public class ArgumentGrouping : List<string>, IGrouping<IImmutableNameSet, string>, IEquatable<IImmutableNameSet>
    {
        internal ArgumentGrouping(IImmutableNameSet key) => Key = key;

        public IImmutableNameSet Key { get; }

        public bool Equals(IImmutableNameSet other)
        {
            return Key.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is IImmutableNameSet nameSet && Equals(nameSet);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}