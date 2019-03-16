using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq.Custom;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Reusable.Utilities.JsonNet
{
    [Obsolete("Use JsonVisitors instead.")]
    [UsedImplicitly]
    public class PrettyTypeReader : JsonTextReader
    {
        private readonly string _typePropertyName;
        private readonly PrettyTypeResolver _prettyTypeResolver;

        private bool _isPrettyType;

        public PrettyTypeReader(TextReader reader, [NotNull] string typePropertyName, [NotNull] IImmutableDictionary<SoftString, Type> types)
            : base(reader)
        {
            _typePropertyName = typePropertyName ?? throw new ArgumentNullException(nameof(typePropertyName));
            _prettyTypeResolver = new PrettyTypeResolver(types ?? throw new ArgumentNullException(nameof(types)));
        }

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
                        SetToken(JsonToken.String, _prettyTypeResolver.Resolve(typeName));
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
}