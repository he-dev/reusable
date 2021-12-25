using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Essentials.Collections;
using Reusable.Essentials.Diagnostics;
using Reusable.Essentials.Extensions;

namespace Reusable.Essentials.Data;

[PublicAPI]
[DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
public class Option<T> : IEnumerable<string>, IEquatable<Option<T>>
{
    public const string Unknown = nameof(Unknown);

    public static readonly IImmutableSet<string> ReservedNames = nameof(None).ToEnumerable().ToImmutableHashSet(SoftString.Comparer);

    // Values are what matters for equality.
    private static readonly IEqualityComparer<Option<T>> Comparer = EqualityComparerFactory<Option<T>>.Create
    (
        equals: (left, right) => left.Values.SetEquals(right.Values),
        getHashCode: (obj) => obj.Values.CalcHashCode()
    );

    private static readonly ConcurrentDictionary<string, Option<T>> Options;

    static Option()
    {
        Options = new ConcurrentDictionary<string, Option<T>>(SoftString.Comparer)
        {
            [nameof(None)] = new(nameof(None), ImmutableHashSet<string>.Empty.Add(nameof(None)))
        };
    }

    private Option(string name, IEnumerable<string> values)
    {
        Name = name;
        Values = values.ToImmutableSortedSet(SoftString.Comparer);
    }

    public string Name
    {
        [DebuggerStepThrough]
        get;
    }

    private IImmutableSet<string> Values { get; }

    private string DebuggerDisplay => ToString(); // this.ToDebuggerDisplayString(b => b.DisplayScalar(x => x.))

    public static Option<T> None => Options[nameof(None)];

    /// <summary>
    /// Gets all known options ever created for this type.
    /// </summary>
    public static IEnumerable<Option<T>> Known => Options.Values;

    /// <summary>
    /// Gets options that have only a single value.
    /// </summary>
    public static IEnumerable<Option<T>> Bits => Options.Values.Where(o => o.IsFlag);


    /// <summary>
    /// Gets value indicating whether this option has only a single value.
    /// </summary>
    public bool IsFlag => Values.Count == 1;

    #region Factories

    public static Option<T> Create(string name, IEnumerable<string> values)
    {
        if (name.In(ReservedNames))
        {
            throw DynamicException.Create("ReservedOption", $"The option '{name}' is reserved and must not be created.");
        }

        if (Options.ContainsKey(name))
        {
            throw DynamicException.Create("DuplicateOption", $"The option '{name}' is already defined.");
        }

        return Options[name] = new Option<T>(name, values);
    }

    public static Option<T> Create(string name, params string[] values)
    {
        return Create(name, values.AsEnumerable());
    }

    public static Option<T> CreateWithCallerName(string? value = default, [CallerMemberName] string name = "")
    {
        return Create(name, value ?? name);
    }

    public static bool TryParse(string? option, [MaybeNullWhen(false)] out Option<T> knownOption)
    {
        if (option is null)
        {
            knownOption = default;
            return false;
        }

        var values =
            Regex
                .Matches(option, @"[a-z0-9_]+", RegexOptions.IgnoreCase)
                .Select(m => m.Value)
                .ToImmutableHashSet(SoftString.Comparer);

        knownOption = Options.Values.SingleOrDefault(o => values.SetEquals(o));

        return knownOption is not null;
    }

    public static Option<T> Parse(string option)
    {
        return
            TryParse(option, out var knownOption)
                ? knownOption
                : throw DynamicException.Create("OptionOutOfRange", $"There is no such option as '{option}'.");
    }

    #endregion

    public Option<T> SetFlag(Option<T> option) => this | option;

    public Option<T> RemoveFlag(Option<T> option) => this ^ option;

    [DebuggerStepThrough]
    public override string ToString() => Values.Select(x => $"{x.ToString()}").Join(", ");

    public bool Contains(Option<T> option) => Values.Overlaps(option);

    #region IEquatable

    public bool Equals(Option<T>? other) => Comparer.Equals(this, other);

    public override bool Equals(object? obj) => Equals(obj as Option<T>);

    public override int GetHashCode() => Comparer.GetHashCode(this);

    #endregion

    public IEnumerator<string> GetEnumerator() => Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Values).GetEnumerator();

    #region Operators

    public static implicit operator string(Option<T> option) => option.ToString();

    public static bool operator ==(Option<T> left, Option<T> right) => Comparer.Equals(left, right);

    public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);

    public static Option<T> operator |(Option<T> left, Option<T> right)
    {
        var values = left.Values.Concat(right.Values).ToImmutableHashSet();
        return GetKnownOrCreate(values);
    }

    public static Option<T> operator ^(Option<T> left, Option<T> right)
    {
        var values = left.Values.Except(right.Values).ToImmutableHashSet();
        return
            values.Any()
                ? GetKnownOrCreate(values)
                : None;
    }

    private static Option<T> GetKnownOrCreate(IImmutableSet<string> values)
    {
        return Options.Values.SingleOrDefault(values.SetEquals) ?? new Option<T>(Unknown, values);
    }

    #endregion
}