using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace Reusable.Essentials;

public static partial class Conditional
{
    [ContractAnnotation("value: null => true; notnull => false")]
    public static bool IsNull<T>(this T? value) where T : class => value is null;

    [ContractAnnotation("value: null => false; notnull => true")]
    public static bool IsNotNull<T>(this T? value) where T : class => value is { };

    [ContractAnnotation("value: null => true; notnull => false")]
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);

    [ContractAnnotation("value: null => false; notnull => true")]
    public static bool IsNotNullOrEmpty(this string? value) => !IsNullOrEmpty(value);

    public static bool Not(this bool value) => !value;
}