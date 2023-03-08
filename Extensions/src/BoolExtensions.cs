using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Reusable.Extensions;

public static partial class BoolExtensions
{
    public static bool Not(this bool value) => !value;
}