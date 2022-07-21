using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Essentials;

namespace Reusable.DoubleDash;

public class NameCollection : IEnumerable<string>, IEquatable<NameCollection>
{
    public NameCollection(string primary, IEnumerable<string>? secondary = default)
    {
        Names = new[] { primary }.Concat((secondary ?? Enumerable.Empty<string>()).Distinct(SoftString.Comparer).OrderBy(n => n)).ToList();
    }

    private List<string> Names { get; }

    public string Primary => Names.First();

    public IEnumerable<string> Secondary => Names.Skip(1);

    public static NameCollection Command => new(nameof(Command));

    public static NameCollection Params => new(nameof(Params));

    public static NameCollection Create(string primary, params string[] secondary) => new(primary, secondary);

    public override int GetHashCode() => this.Join(" | ").GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as NameCollection);

    // Collections are equal when there is an intersection.
    public bool Equals(NameCollection? other) => Names.Intersect(other ?? Enumerable.Empty<string>(), SoftString.Comparer).Any();

    public IEnumerator<string> GetEnumerator() => Names.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Names).GetEnumerator();
}