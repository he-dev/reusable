using System;
using JetBrains.Annotations;

namespace Reusable.Translucent.Annotations;

[UsedImplicitly]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
public class ControllerNameAttribute : Attribute
{
    public ControllerNameAttribute(string value) => Value = value;

    public string Value { get; }

    public override string ToString() => Value;

    public static implicit operator string(ControllerNameAttribute attr) => attr.Value;
}