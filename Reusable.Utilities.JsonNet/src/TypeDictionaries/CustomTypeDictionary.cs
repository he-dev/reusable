using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Utilities.JsonNet.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Utilities.JsonNet.TypeDictionaries;

public class CustomTypeDictionary : ITypeDictionary
{
    private readonly IImmutableDictionary<string, Type> _types;

    public CustomTypeDictionary(IEnumerable<Type> types)
    {
        var items =
            from type in types
            let prettyName = type.ToPrettyString()
            where !IsDynamicType(prettyName)
            from schema in type.GetCustomAttributes<JsonTypeSchemaAttribute>().FirstOrDefault() ?? throw DynamicException.Create
            (
                $"JsonTypeSchemaNotFound",
                $"Custom types must be decorated with the {nameof(JsonTypeSchemaAttribute)}."
            )
            select (prettyName: $"{schema}.{prettyName}", type);

        _types = items.ToImmutableSortedDictionary(t => t.prettyName, t => t.type, SoftString.Comparer);
    }

    public CustomTypeDictionary(params Type[] types) : this(types.AsEnumerable()) { }

    public Type? Resolve(string typeName)
    {
        return _types.TryGetValue(typeName, out var type) ? type : default;
    }

    private static bool IsDynamicType(string name)
    {
        return name.StartsWith("<>f__AnonymousType") || name.StartsWith("<>c__DisplayClass");
    }

    private static void ValidateHasNoGenericArguments(Type type)
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

    public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() => _types.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_types).GetEnumerator();
}