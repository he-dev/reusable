using System;
using System.Collections.Generic;

namespace Reusable;

public class StringComparerLite :
    IEqualityComparer<string?>,
    IComparer<string?>
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    public bool Equals(string? x, string? y) => Comparer.Equals(x?.Trim(), y?.Trim());

    public int GetHashCode(string obj) => Comparer.GetHashCode(obj.Trim());

    public int Compare(string? x, string? y) => Comparer.Compare(x, y);
}