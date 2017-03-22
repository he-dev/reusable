using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public class StringSet : HashSet<string>
    {
        protected StringSet(IEnumerable<string> values, IEqualityComparer<string> comparer)
        : base(
              values ?? throw new ArgumentNullException(nameof(values)),
              comparer
        )
        { }

        public static StringSet CreateCI(IEnumerable<string> values) => new StringSet(values, StringComparer.OrdinalIgnoreCase);

        public static StringSet CreateCI(params string[] values) => CreateCI((IEnumerable<string>)values);

        public static StringSet CreateCS(IEnumerable<string> values) => new StringSet(values, StringComparer.Ordinal);

        public static StringSet CreateCS(params string[] values) => CreateCI((IEnumerable<string>)values);
    }



    //public class StringSetCI : StringSet
    //{
    //    private StringSetCI(IEnumerable<string> keys, IEqualityComparer<string> keyComparer)
    //    : base(keys, keyComparer)
    //    { }

    //    public static StringSetCI Create(params string[] keys) => new StringSetCI(keys, StringComparer.OrdinalIgnoreCase);

    //    public static bool operator ==(StringSetCI left, StringSetCI right) => !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Overlaps(right);

    //    public static bool operator !=(StringSetCI left, StringSetCI right) => !(left == right);
    //}

    //public class StringSetCS : StringSet
    //{
    //    private StringSetCS(IEnumerable<string> keys, IEqualityComparer<string> keyComparer)
    //    : base(keys, keyComparer)
    //    { }

    //    public static StringSetCS Create(params string[] keys) => new StringSetCS(keys, StringComparer.Ordinal);

    //    public static bool operator ==(StringSetCS left, StringSetCS right) => !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Overlaps(right);

    //    public static bool operator !=(StringSetCS left, StringSetCS right) => !(left == right);
    //}

    public class HashSetOverlapsComparer<T> : IEqualityComparer<HashSet<T>>
    {
        public bool Equals(HashSet<T> x, HashSet<T> y) => x.Overlaps(y);

        public int GetHashCode(HashSet<T> obj) => obj.Comparer.GetHashCode(); // Force Equals for similar comparers.
    }
}
