using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;

namespace Reusable.DoubleDash;

public class ArgumentName : IEnumerable<string>, IEquatable<ArgumentName>
{
    private readonly List<string> _names;

    public ArgumentName(string primary, IEnumerable<string>? secondary = default)
    {
        _names = new[] { primary }.Concat((secondary ?? Enumerable.Empty<string>()).Distinct(SoftString.Comparer).OrderBy(n => n)).ToList();
    }

    public string Primary => _names.First();

    public IEnumerable<string> Secondary => _names.Skip(1);

    public static ArgumentName Command => new ArgumentName(nameof(Command));
        
    public static ArgumentName Params => new ArgumentName(nameof(Params));

    public static ArgumentName Create(string primary, params string[] secondary) => new ArgumentName(primary, secondary);

    public override int GetHashCode() => 0;

    public override bool Equals(object? obj) => Equals(obj as ArgumentName);

    public bool Equals(ArgumentName? other) => _names.Intersect(other ?? Enumerable.Empty<string>(), SoftString.Comparer).Any();

    public IEnumerator<string> GetEnumerator() => _names.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_names).GetEnumerator();
}