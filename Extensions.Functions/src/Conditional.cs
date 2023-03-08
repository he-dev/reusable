using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Reusable.Extensions;

public static partial class Conditional
{
    [ContractAnnotation("value: null => true; notnull => false")]
    public static bool IsNull<T>([NotNullWhen(false)] this T? value) where T : class => value is null;

    [ContractAnnotation("value: null => false; notnull => true")]
    public static bool IsNotNull<T>([NotNullWhen(true)] this T? value) where T : class => value is { };

    

    public static bool Not(this bool value) => !value;
}