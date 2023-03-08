using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Reusable;

/// <summary>
/// This special string class represents a string that is trimmed and implements case-insensitive comparison.
/// </summary>
[Serializable]
public partial class SoftString : IEnumerable<char>
{
    public static readonly SoftStringComparer Comparer = new();

    private readonly string _value;

    [DebuggerStepThrough]
    public SoftString(string? value) => _value = value?.Trim() ?? string.Empty;

    public SoftString() : this(string.Empty) { }

    [DebuggerStepThrough]
    public SoftString(char value) : this(value.ToString()) { }
        
    public static SoftString Empty { get; } = new();

    public char this[int index] => _value[index];

    public int Length => _value.Length;
    
    public static SoftString? Create(string? value) => value is {} ? new SoftString(value) : default;

    public override string ToString() => _value;

    #region IEnumerable

    public IEnumerator<char> GetEnumerator() => ToString().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}