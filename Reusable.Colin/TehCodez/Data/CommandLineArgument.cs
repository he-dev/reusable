using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.CommandLine.Collections;

namespace Reusable.CommandLine.Data
{
    /// <summary>
    /// This class represents a single command-line argument with all its values.
    /// </summary>
    public class CommandLineArgument : List<string>, IGrouping<IImmutableNameSet, string>, IEquatable<IImmutableNameSet>
    {
        internal CommandLineArgument(IImmutableNameSet key) => Key = key;

        public IImmutableNameSet Key { get; }

        public bool Equals(IImmutableNameSet other)
        {
            return ImmutableNameSet.Comparer.Equals(Key, other);
        }

        public override bool Equals(object obj)
        {
            return obj is IImmutableNameSet nameSet && Equals(nameSet);
        }

        public override int GetHashCode()
        {
            return ImmutableNameSet.Comparer.GetHashCode(Key);
        }
    }
}