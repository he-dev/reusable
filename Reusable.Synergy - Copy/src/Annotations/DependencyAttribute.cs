using System;
using JetBrains.Annotations;

namespace Reusable.Synergy.Annotations;

// Marks property that requires an external dependency.
[UsedImplicitly]
[AttributeUsage(AttributeTargets.Property)]
public class DependencyAttribute : Attribute
{
    public bool Required { get; set; } = true;
}