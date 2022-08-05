using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Reusable.Marbles;
using Reusable.Marbles.Extensions;
using Reusable.Utilities.JsonNet.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;
using Reusable.Utilities.JsonNet.TypeDictionaries;

namespace Reusable.Utilities.JsonNet;

public static class TypeDictionaryExtensions
{
    public static ITypeDictionary Add(this ITypeDictionary typeDictionary, ITypeDictionary other) => new CompositeTypeDictionary(typeDictionary, other);
}

public static class PrettyTypeDictionary
{
    /// <summary>
    /// Gets a dictionary of primitive types.
    /// </summary>
    public static readonly IImmutableDictionary<string, Type> BuiltInTypes =
        ImmutableDictionary
            .Create<string, Type>(SoftString.Comparer)
            .Add("bool", typeof(Boolean))
            .Add("byte", typeof(Byte))
            .Add("sbyte", typeof(SByte))
            .Add("char", typeof(Char))
            .Add("decimal", typeof(Decimal))
            .Add("double", typeof(Double))
            .Add("float", typeof(Single))
            .Add("int", typeof(Int32))
            .Add("uint", typeof(UInt32))
            .Add("long", typeof(Int64))
            .Add("ulong", typeof(UInt64))
            .Add("object", typeof(Object))
            .Add("short", typeof(Int16))
            .Add("ushort", typeof(UInt16))
            .Add("string", typeof(String));

    public static IImmutableDictionary<string, Type> From(IEnumerable<Assembly> assemblies)
    {
        var assemblyTypes =
            from assembly in assemblies
            from type in assembly.GetTypes()
            select type;

        return From(assemblyTypes);
    }

    public static IImmutableDictionary<string, Type> From(IEnumerable<Type> types, IComparer<string>? keyComparer = default)
    {
        keyComparer ??= SoftString.Comparer;

        var items =
            from type in types.ForEach(t => t.ValidateHasNoGenericArguments())
            let prettyName = type.ToPrettyString()
            where !prettyName.IsDynamicType()
            // Pick the nearest namespace-attribute or one derived from it. 
            let ns = type.GetCustomAttributes<JsonTypeSchemaAttribute>().FirstOrDefault()
            select
                ns is null
                    ? new[] { (prettyName, type) }
                    : ns.Select(n => (prettyName: $"{n}.{prettyName}", type));

        return items.SelectMany(x => x).ToImmutableSortedDictionary(t => t.prettyName, t => t.type, keyComparer);
    }

    public static IImmutableDictionary<string, Type> From(params Type[] types) => From(types.AsEnumerable());

    private static bool IsDynamicType(this string name)
    {
        return name.StartsWith("<>f__AnonymousType") || name.StartsWith("<>c__DisplayClass");
    }

    private static void ValidateHasNoGenericArguments(this Type type)
    {
        if (type.GenericTypeArguments.Any())
        {
            throw DynamicException.Create
            (
                "GenericTypeArguments",
                "You must not specify generic type arguments explicitly. Leave them empty, e.g. 'List<>' or 'Dictionary<,>'"
            );
        }
    }
}