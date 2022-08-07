﻿using System;
using Reusable.Marbles.Extensions;

namespace Reusable.Marbles;

public partial class SoftString : IComparable<SoftString>, IComparable<string>
{
    public int CompareTo(SoftString? other) => Comparer.Compare(this, other);

    public int CompareTo(string? other) => CompareTo(other.ToSoftString());
}