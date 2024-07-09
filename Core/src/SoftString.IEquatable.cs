using System;
using System.Diagnostics;

namespace Reusable;

public partial class SoftString : IEquatable<SoftString>, IEquatable<string>
{
    [DebuggerStepThrough]
    public override int GetHashCode() => Comparer.GetHashCode(this);

    [DebuggerStepThrough]
    public override bool Equals(object? obj) => Equals(obj as SoftString);

    [DebuggerStepThrough]
    public bool Equals(SoftString? other) => Comparer.Equals(this, other);

    [DebuggerStepThrough]
    public bool Equals(string? other) => Equals(other.ToSoftString());
}