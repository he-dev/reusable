using System;
using System.Collections.Generic;

namespace Reusable
{
    public class SoftStringComparer : IEqualityComparer<SoftString>, IEqualityComparer<string>, IComparer<SoftString>, IComparer<string>
    {
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        public bool Equals(SoftString x, SoftString y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return Comparer.Equals(x.ToString(), y.ToString());
        }

        public bool Equals(string x, string y) => Equals((SoftString)x, (SoftString)y);

        public int GetHashCode(SoftString obj)
        {
            return Comparer.GetHashCode(obj.ToString());
        }

        public int GetHashCode(string obj) => GetHashCode((SoftString)obj);

        public int Compare(SoftString x, SoftString y)
        {
            return Comparer.Compare(x?.ToString(), y?.ToString());
        }
        
        public int Compare(string x, string y)
        {
            return Comparer.Compare((SoftString)x, (SoftString)y);
        }
    }
}