using System;
using JetBrains.Annotations;

namespace Reusable.Collections;

[UsedImplicitly]
[AttributeUsage(AttributeTargets.Property)]
public class AutoEqualityPropertyAttribute : Attribute
{
    public AutoEqualityPropertyAttribute() { }
    public AutoEqualityPropertyAttribute(StringComparison stringComparison) => StringComparison = stringComparison;
    public StringComparison StringComparison { get; } = StringComparison.CurrentCulture;
}