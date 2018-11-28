using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Reusable.Utilities.JsonNet
{
    public delegate JToken VisitJsonCallback(JToken jToken);

    public class JsonVisitor
    {
        public static JToken Visit(JToken jToken, params JsonVisitor[] visitors)
        {
            return visitors.Aggregate(jToken, (current, visitor) => visitor.Visit(current));
        }

        public static VisitJsonCallback Create(params JsonVisitor[] visitors) => jToken => Visit(jToken, visitors);

        public JToken Visit(JToken jToken)
        {
            return
                jToken is JValue
                    ? jToken
                    : VisitInternal(jToken, CreateJContainer(jToken.Type));
        }

        private JToken VisitInternal(JToken jToken, JContainer result)
        {
            if (jToken is JValue)
            {
                return jToken;
            }

            foreach (var element in jToken)
            {
                switch (element)
                {
                    case JProperty jProperty:
                        var visitedProperty = VisitProperty(jProperty.Name, Visit(jProperty.Value));
                        result.Add(visitedProperty);
                        break;
                    default:
                        result.Add(Visit(element));
                        break;
                }
            }
            return result;
        }

        protected virtual JProperty VisitProperty(string name, JToken value) => new JProperty(name, value);

        private static JContainer CreateJContainer(JTokenType type)
        {
            switch (type)
            {
                case JTokenType.Object: return new JObject();
                case JTokenType.Array: return new JArray();
                default: throw new ArgumentOutOfRangeException($"Invalid type: {type}");
            }
        }
    }

    public class PropertyNameTrimmer : JsonVisitor
    {
        protected override JProperty VisitProperty(string name, JToken value)
        {
            return new JProperty(name.Trim(), value);
        }
    }

    public class TypeResolver : JsonVisitor
    {
        private readonly string _typePropertyName;
        private readonly Func<string, string> _resolveType;

        public TypeResolver(string typePropertyName, Func<string, string> resolveType)
        {
            _typePropertyName = typePropertyName;
            _resolveType = resolveType;
        }

        protected override JProperty VisitProperty(string name, JToken value)
        {
            return
                name == _typePropertyName
                    ? new JProperty("$type", _resolveType(value.Value<string>()))
                    : base.VisitProperty(name, value);
        }
    }
}
