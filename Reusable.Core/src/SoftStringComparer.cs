using System;
using System.Collections.Generic;

namespace Reusable
{
    public class SoftStringComparer : IEqualityComparer<SoftString>, IComparer<SoftString>
    {
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        public bool Equals(SoftString x, SoftString y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return Comparer.Equals(x.ToString(), y.ToString());
        }

        public int GetHashCode(SoftString obj)
        {
            return Comparer.GetHashCode(obj.ToString());
        }

        public int Compare(SoftString x, SoftString y)
        {
            return Comparer.Compare(x?.ToString(), y?.ToString());
        }
    }
}