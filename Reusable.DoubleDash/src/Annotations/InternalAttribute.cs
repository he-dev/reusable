using System;
using JetBrains.Annotations;

namespace Reusable.DoubleDash.Annotations;

/// <summary>
/// Specifies that a command is an internal one and not callable by command-line.
/// </summary>
[UsedImplicitly]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
public class InternalAttribute : Attribute { }