using System;

namespace Reusable.Extensions;

public static class RangeExtensions
{
    public static bool Contains(this Range range, int value) => range.Start.Value <= value && value < range.End.Value;
}