using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Reusable.Marbles;

public static partial class Conditional
{
    [ContractAnnotation("value: null => true; notnull => false")]
    public static bool IsNull<T>([NotNullWhen(false)] this T? value) where T : class => value is null;

    [ContractAnnotation("value: null => false; notnull => true")]
    public static bool IsNotNull<T>([NotNullWhen(true)] this T? value) where T : class => value is { };

    [ContractAnnotation("value: null => true; notnull => false")]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

    [ContractAnnotation("value: null => false; notnull => true")]
    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? value) => !IsNullOrEmpty(value);

    public static bool Not(this bool value) => !value;
}