using System;
using System.Collections.Generic;
using Reusable.Extensions;

namespace Reusable
{
    public class SoftStringComparer : IEqualityComparer<SoftString?>, IEqualityComparer<string?>, IComparer<SoftString?>
    {
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

        public bool Equals(SoftString? x, SoftString? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return Comparer.Equals(x.ToString(), y.ToString());
        }

        public bool Equals(string? x, string? y) => Equals(x.ToSoftString(), y.ToSoftString());

        public int GetHashCode(SoftString? obj)
        {
            return obj is {} ? Comparer.GetHashCode(obj.ToString()) : 0;
        }

        public int GetHashCode(string? obj) => GetHashCode(obj.ToSoftString());

        public int Compare(SoftString? x, SoftString? y)
        {
            return Comparer.Compare(x?.ToString(), y?.ToString());
        }
    }
}