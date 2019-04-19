using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Utilities.JsonNet
{
    public static class TypeDictionary
    {
        public static readonly IImmutableDictionary<SoftString, Type> BuiltInTypes =
            ImmutableDictionary
                .Create<SoftString, Type>()
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

        public static IImmutableDictionary<SoftString, Type> From(IEnumerable<Assembly> assemblies)
        {
            var assemblyTypes =
                from assembly in assemblies
                from type in assembly.GetTypes()
                select type;

            return From(assemblyTypes);
        }

        public static IImmutableDictionary<SoftString, Type> From(IEnumerable<Type> types)
        {
            return
            (
                from type in types.Select(ValidateHasNoGenericArguments)
                // Pick the nearest namespace-attribute or one derived from it. 
                let prefix = type.GetCustomAttributes<NamespaceAttribute>().FirstOrDefault()?.ToString()
                let prettyName = type.ToPrettyString()
                where !prettyName.IsDynamicType()
                //from name in new[] { prettyName, type.GetCustomAttribute<AliasAttribute>()?.ToString() }
                //where name.IsNotNullOrEmpty()
                select (type, prettyName: SoftString.Create((prefix.IsNullOrEmpty() ? string.Empty : $"{prefix}.") + prettyName))
            ).ToImmutableDictionary(t => t.prettyName, t => t.type);
        }

        public static IImmutableDictionary<SoftString, Type> From(params Type[] types)
        {
            return From((IEnumerable<Type>)types);
        }

        private static bool IsDynamicType(this string name)
        {
            return name.StartsWith("<>f__AnonymousType") || name.StartsWith("<>c__DisplayClass");
        }

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