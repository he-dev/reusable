using System;
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
}