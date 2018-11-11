using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Utilities.JsonNet
{
    [UsedImplicitly]
    public class PrettyTypeReader : JsonTextReader
    {
        private readonly string _typePropertyName;
        private readonly PrettyTypeExpander _prettyTypeExpander;

        private bool _isPrettyType;

        public PrettyTypeReader(TextReader reader, [NotNull] string typePropertyName, [NotNull] Func<string, Type> resolvePrettyType)
            : base(reader)
        {
            _typePropertyName = typePropertyName ?? throw new ArgumentNullException(nameof(typePropertyName));
            _prettyTypeExpander = new PrettyTypeExpander(resolvePrettyType ?? throw new ArgumentNullException(nameof(resolvePrettyType)));
        }

        //public PrettyTypeReader(TextReader reader, params Type[] assemblyProviders)
        //    : this(reader, AbbreviatedTypePropertyName, PrettyTypeResolver.Create(assemblyProviders))
        //{
        //}

        private const string DefaultTypePropertyName = "$type";

        public const string AbbreviatedTypePropertyName = "$t";

        public override bool Read()
        {
            if (base.Read() is var hasToken)
            {
                switch (TokenType)
                {
                    // Replace custom type-property-name with the default one, e.g. "-t" -> "$type"
                    case JsonToken.PropertyName when IsCustomTypePropertyName(Value):
                        SetToken(JsonToken.PropertyName, DefaultTypePropertyName);
                        _isPrettyType = true;
                        break;

                    // Expand type name definition, e.g. "MyType" -> "Namespace.MyType, Assembly"
                    case JsonToken.String when _isPrettyType && Value is string typeName:
                        SetToken(JsonToken.String, _prettyTypeExpander.Expand(typeName));
                        break;

                    default:
                        _isPrettyType = false;
                        break;
                }
            }

            return hasToken;
        }

        private bool IsCustomTypePropertyName(object value)
        {
            return value is string propertyName && propertyName.Equals(_typePropertyName);
        }
    }

    internal class PrettyTypeExpander
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
        private const string PrettyTypePattern = @"(?<type>(?i)[a-z0-9_.]+)(?:\<(?<genericArguments>(?i)[a-z0-9_., ]+)\>)?";

        private readonly Func<string, Type> _resolveType;

        public PrettyTypeExpander([NotNull] Func<string, Type> resolveType)
        {
            _resolveType = resolveType;
        }

        public string Expand(string prettyType)
        {
            var match =
                Regex
                    .Match(prettyType, PrettyTypePattern, RegexOptions.ExplicitCapture)
                    .OnFailure(_ => new ArgumentException($"Invalid type alias: '{prettyType}'."));

            var genericArguments = CreateGenericArguments(match.Groups["genericArguments"]);
            var type = ResolveType($"{match.Groups["type"].Value}{genericArguments.Signature}");
            return $"{type.FullName}{genericArguments.FullName}, {type.Assembly.GetName().Name}";
        }

        // Signature: "<, >"
        // FullName: "[[T1],[T2]]" - the "`2" prefix is provided by type-full-name later 
        private (string Signature, string FullName) CreateGenericArguments(Group genericArguments)
        {
            if (genericArguments.Success)
            {
                // "<, >"
                var commas = string.Join(string.Empty, genericArguments.Value.Where(c => c == ',').Select(c => $"{c} "));
                var signature = $"<{commas}>";

                var genericArgumentNames = genericArguments.Value.Split(',').Select(x => x.Trim()).ToList();
                var genericArgumentFullNames =
                (
                    from name in genericArgumentNames
                    let genericType = GetTypeByAlias(name) ?? ResolveType(name)
                    select $"[{genericType.FullName}, {genericType.Assembly.GetName().Name}]"
                );

                // Creates: "[[Namespace.T1, ...],[Namespace.T2, ...]]"
                var fullName = $"[{string.Join(",", genericArgumentFullNames)}]";

                return (signature, fullName);
            }
            else
            {
                return (default, default);
            }
        }

        private static Type GetTypeByAlias(string name) => Types.TryGetValue(name, out var t) ? t : default;

        private Type ResolveType(string typeName)
        {
            return
                _resolveType(typeName) ??
                Type.GetType(typeName, ignoreCase: true, throwOnError: false) ??
                throw DynamicException.Create("TypeNotFound", $"Could not resolve '{typeName}'.");
        }
    }
}