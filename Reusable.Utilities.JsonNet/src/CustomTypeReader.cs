using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Utilities.JsonNet
{
    [UsedImplicitly]
    public class CustomTypeReader : JsonTextReader
    {
        private readonly string _typePropertyName;
        private readonly TypeAliasExpander _typeAliasExpander;

        private (JsonToken TokenType, object Value) _last;

        public CustomTypeReader(TextReader reader, [NotNull] string typePropertyName, [NotNull] Func<string, Type> typeResolver)
            : base(reader)
        {
            _typePropertyName = typePropertyName ?? throw new ArgumentNullException(nameof(typePropertyName));
            _typeAliasExpander = new TypeAliasExpander(typeResolver ?? throw new ArgumentNullException(nameof(typeResolver)));
        }

        public const string DefaultTypePropertyName = "$type";

        public override bool Read()
        {
            if (base.Read() is var hasToken)
            {
                switch (TokenType)
                {
                    // Replace custom type-property-name with the default one, e.g. "-t" -> "$type"
                    case JsonToken.PropertyName when IsCustomTypePropertyName(Value):
                        SetToken(JsonToken.PropertyName, DefaultTypePropertyName);
                        break;

                    // Expand type name definition, e.g. "MyType" -> "Namespace.MyType, Assembly"
                    case JsonToken.String when IsTypeName() && Value is string typeName && IsTypeNameAlias(typeName):
                        SetToken(JsonToken.String, _typeAliasExpander.ExpandTypeAlias(typeName));
                        break;
                }
            }

            _last = (TokenType, Value);

            return hasToken;
        }

        private bool IsCustomTypePropertyName(object value)
        {
            return value is string propertyName && propertyName.Equals(_typePropertyName);
        }

        private bool IsTypeNameAlias(string typeName)
        {
            // Type-name-alias don't use "," to specify the assembly.
            return typeName.IndexOf(',') < 0;
        }

        private bool IsTypeName()
        {
            return
                _last.TokenType == JsonToken.PropertyName &&
                _last.Value is string value &&
                value.In(DefaultTypePropertyName, _typePropertyName);
        }
    }

    internal class TypeAliasExpander
    {
        private static readonly IImmutableDictionary<string, Type> Types =
            ImmutableDictionary
                .Create<string, Type>(StringComparer.OrdinalIgnoreCase)
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

        // Used to specify user-friendly type names like: "List<int>" instead of "List`1[System.Int32...]" etc.
        // https://regex101.com/r/QZ5T5I/1/
        // language=regexp
        private const string TypeAliasPattern = @"(?<type>(?i)[a-z0-9_.]+)(?:\<(?<genericArguments>(?i)[a-z0-9_., ]+)\>)?";

        public TypeAliasExpander([NotNull] Func<string, Type> typeResolver)
        {
            TypeResolver = typeResolver;
        }

        private Func<string, Type> TypeResolver { get; }

        public string ExpandTypeAlias(string value)
        {
            var match = Regex.Match(value, TypeAliasPattern, RegexOptions.ExplicitCapture);
            if (match.Success)
            {
                var type = ResolveType(match.Groups["type"].Value);
                var genericArguments = CreateGenericArguments(match.Groups["genericArguments"]);
                // Creates: "Namespace.Type`2[[T1],[T2]], Assembly"
                return $"{type.FullName}{genericArguments}, {type.Assembly.GetName().Name}";
            }

            throw new ArgumentException("Invalid '$type' name. You must provide either a full json.net type or an alias.");
        }

        private string CreateGenericArguments(Group genericArguments)
        {
            if (genericArguments.Success)
            {
                var genericArgumentNames =
                (
                    from name in genericArguments.Value.Split(',').Select(x => x.Trim())
                    let genericType = GetTypeByAlias(name) ?? ResolveType(name)
                    select $"[{genericType.FullName}, {genericType.Assembly.GetName().Name}]"
                ).ToList();

                // Creates: "`2[[Namespace.T1, ...],[Namespace.T2, ...]]"                
                return $"`{genericArgumentNames.Count}[{string.Join(",", genericArgumentNames)}]";
            }

            return string.Empty;

            Type GetTypeByAlias(string name) => Types.TryGetValue(name, out var t) ? t : default;
        }

        private Type ResolveType(string typeName)
        {
            return Type.GetType
            (
                typeName: typeName,
                assemblyResolver: default, // does not get called
                typeResolver: (assembly, typeName2, ignoreCase) => TypeResolver(typeName2),
                throwOnError: true
            );
        }
    }

    public static class TypeResolver
    {
        public static Func<string, Type> Create(params Type[] assemblyProviders)
        {
            var types = assemblyProviders.SelectMany(t => t.Assembly.GetTypes());
            return typeName => types.SingleOrDefault(t => SoftString.Comparer.Equals(t.Name, typeName));
        }
    }
}