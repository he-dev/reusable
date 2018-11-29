using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Reusable.Utilities.JsonNet
{
    public delegate JToken VisitJsonCallback(JToken token);

    public class JsonVisitor
    {
        public static JToken Visit(JToken token, params JsonVisitor[] visitors)
        {
            return visitors.Aggregate(token, (current, visitor) => visitor.Visit(current));
        }

        public static VisitJsonCallback Create(params JsonVisitor[] visitors) => jToken => Visit(jToken, visitors);

        public JToken Visit(JToken token)
        {
            return
                token is JValue
                    ? token
                    : VisitInternal(token, CreateJContainer(token.Type));
        }

        private JToken VisitInternal(JToken token, JContainer result)
        {
            if (token is JValue)
            {
                return token;
            }

            foreach (var element in token)
            {
                switch (element)
                {
                    case JProperty property:
                        var visitedProperty = VisitProperty(property);
                        result.Add(visitedProperty);
                        break;
                    default:
                        result.Add(Visit(element));
                        break;
                }
            }
            return result;
        }

        protected virtual JProperty VisitProperty(JProperty property)
        {
            return new JProperty(property.Name, Visit(property.Value));
        }

        private static JContainer CreateJContainer(JTokenType tokenType)
        {
            switch (tokenType)
            {
                case JTokenType.Object: return new JObject();
                case JTokenType.Array: return new JArray();
                default:
                    throw new ArgumentOutOfRangeException
                    (
                        paramName: nameof(tokenType),
                        message: $"Invalid token type: {tokenType} (supported are only [{string.Join(", ", new[] { JTokenType.Object, JTokenType.Array })}]"
                    );
            }
        }
    }

    public class PropertyNameTrimmer : JsonVisitor
    {
        protected override JProperty VisitProperty(JProperty property)
        {
            return new JProperty(property.Name.Trim(), Visit(property.Value));
        }
    }

    public class TypeResolver : JsonVisitor
    {
        private readonly string _typePropertyName;
        private readonly Func<string, string> _resolveType;

        public const string DefaultTypePropertyName = "$type";

        public TypeResolver(string typePropertyName, Func<string, string> resolveType)
        {
            _typePropertyName = typePropertyName;
            _resolveType = resolveType;
        }

        protected override JProperty VisitProperty(JProperty property)
        {
            return
                property.Name.Equals(_typePropertyName, StringComparison.OrdinalIgnoreCase)
                    ? new JProperty(DefaultTypePropertyName, _resolveType(property.Value.Value<string>()))
                    : base.VisitProperty(property);
        }
    }

    public class PrettyTypeResolver : TypeResolver
    {
        public PrettyTypeResolver(IEnumerable<Type> types)
            : base("$t", new PrettyTypeExpander(PrettyTypeResolverCallback.Create(types)).Expand)
        { }
    }

    public class CSharpNamingConventionValidator : JsonVisitor
    {
        protected override JProperty VisitProperty(JProperty property)
        {
            if (!Regex.IsMatch("[a-z0-9_][a-z0-9_]*", property.Name, RegexOptions.IgnoreCase))
            {
                throw new ArgumentException(paramName: nameof(property), message: "Invalid property name.");
            }
            return base.VisitProperty(property);
        }
    }
}
