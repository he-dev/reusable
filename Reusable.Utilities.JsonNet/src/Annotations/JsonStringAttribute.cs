using System;
using JetBrains.Annotations;

namespace Reusable.Utilities.JsonNet.Annotations;

[UsedImplicitly]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class JsonStringAttribute : Attribute { }