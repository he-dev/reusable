using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Utilities.JsonNet
{
    public static class PrettyTypeResolverCallback
    {
        public static Func<string, Type> Create(IEnumerable<Assembly> assemblies)
        {
            var typeDictionary =
            (
                from assembly in assemblies
                from type in assembly.GetTypes()
                let prettyName = type.ToPrettyString().ToSoftString()
                where !prettyName.IsDynamicType()
                select (type, prettyName)
            )
            .ToDictionarySafely(t => t.prettyName, t => t.type, SoftString.Comparer);

            return prettyName => typeDictionary.TryGetValue(prettyName, out var type) ? type : default;
        }

        public static Func<string, Type> Create(IEnumerable<Type> types)
        {
            var typeDictionary =
            (
                from type in types.Select(ValidateHasNoGenericArguments)
                let prettyName = type.ToPrettyString().ToSoftString()
                where !prettyName.IsDynamicType()
                select (type, prettyName)
            )
            .ToDictionarySafely(t => t.prettyName, t => t.type, SoftString.Comparer);

            return prettyName => typeDictionary.TryGetValue(prettyName, out var type) ? type : default;
        }

        private static bool IsDynamicType(this SoftString name) => name.StartsWith("<>f__AnonymousType") || name.StartsWith("<>c__DisplayClass");

        private static Type ValidateHasNoGenericArguments(this Type type)
        {
            return
                type.GenericTypeArguments.Any()
                    ? throw DynamicException.Create
                    (
                        "GenericTypeArguments",
                        "You must not specify generic type arguments explicitly. Leave them empty, e.g. 'List<>' or 'Dictionary<,>'"
                    )
                    : type;
        }
    }
}