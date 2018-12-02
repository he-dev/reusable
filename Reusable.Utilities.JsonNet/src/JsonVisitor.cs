using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Reusable.Reflection;

namespace Reusable.Utilities.JsonNet
{
    public interface IJsonVisitor
    {
        [NotNull]
        JToken Visit([NotNull] JToken token);
    }

    public abstract class JsonVisitor : IJsonVisitor
    {
        public static JsonVisitor CreateComposite(params JsonVisitor[] visitors) => new CompositeJsonVisitor(visitors);

        public virtual JToken Visit(JToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            return
                token is JValue
                    ? token
                    : VisitInternal(token, CreateJContainer(token.Type));
        }

        public JToken Visit(string json) => Visit(JToken.Parse(json));

        [NotNull]
        private JToken VisitInternal(JToken token, JContainer result)
        {
            foreach (var element in token)
            {
                switch (element)
                {
                    case JProperty property:
                        var visitedProperty = VisitProperty(property);
                        result.Add(visitedProperty);
                        break;
                    case JValue value:
                        result.Add(value);
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

    internal class CompositeJsonVisitor : JsonVisitor
    {
        private readonly Func<JToken, JToken> _visit;

        public CompositeJsonVisitor(params JsonVisitor[] visitors)
        {
            _visit = token => visitors.Aggregate(token, (current, visitor) => visitor.Visit(current));
        }

        public override JToken Visit(JToken token) => _visit(token);
    }

    public static class JsonVisitorExtensions
    {
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

        public TypeResolver([NotNull] string typePropertyName, [NotNull] Func<string, string> resolveType)
        {
            _typePropertyName = typePropertyName ?? throw new ArgumentNullException(nameof(typePropertyName));
            _resolveType = resolveType ?? throw new ArgumentNullException(nameof(resolveType));
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

    public class PropertyNameValidator : JsonVisitor
    {
        private readonly IImmutableSet<string> _propertyNames;
        public PropertyNameValidator(IEnumerable<string> propertyNames)
        {
            _propertyNames = propertyNames.ToImmutableHashSet();
        }

        protected override JProperty VisitProperty(JProperty property)
        {
            return
                _propertyNames.Contains(property.Name)
                    ? base.VisitProperty(property)
                    : throw DynamicException.Create("UnknownPropertyName", $"Property '{property.Name}' is not allowed.");
        }
    }

    //public class CSharpNamingConventionValidator : JsonVisitor
    //{
    //    protected override JProperty VisitProperty(JProperty property)
    //    {
    //        if (!Regex.IsMatch("[a-z0-9_][a-z0-9_]*", property.Name, RegexOptions.IgnoreCase))
    //        {
    //            throw new ArgumentException(paramName: nameof(property), message: "Invalid property name.");
    //        }
    //        return base.VisitProperty(property);
    //    }
    //}
}
