using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public class StringSet : HashSet<string>
    {
        protected StringSet(IEnumerable<string> keys, IEqualityComparer<string> keyComparer)
        : base(keys ?? throw new ArgumentNullException(nameof(keys)), keyComparer)
        { }
    }

    public class StringSetCI : StringSet
    {
        private StringSetCI(IEnumerable<string> keys, IEqualityComparer<string> keyComparer)
        : base(keys, keyComparer)
        { }

        public static StringSetCI Create(params string[] keys) => new StringSetCI(keys, StringComparer.OrdinalIgnoreCase);

        public static bool operator ==(StringSetCI left, StringSetCI right) => !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Overlaps(right);

        public static bool operator !=(StringSetCI left, StringSetCI right) => !(left == right);
    }

    public class StringSetCS : StringSet
    {
        private StringSetCS(IEnumerable<string> keys, IEqualityComparer<string> keyComparer)
        : base(keys, keyComparer)
        { }

        public static StringSetCS Create(params string[] keys) => new StringSetCS(keys, StringComparer.Ordinal);

        public static bool operator ==(StringSetCS left, StringSetCS right) => !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Overlaps(right);

        public static bool operator !=(StringSetCS left, StringSetCS right) => !(left == right);
    }

    public class SetOverlapsComparer<T> : IEqualityComparer<ISet<T>>
    {
        public bool Equals(ISet<T> x, ISet<T> y) => x.Overlaps(y);

        public int GetHashCode(ISet<T> obj) => 0; // Force Equals.
    }
}
