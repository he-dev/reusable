using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionizer;
using Reusable.Reflection;

namespace Reusable.Utilities.JsonNet
{
    public interface IJsonVisitor
    {
        [NotNull]
        JToken Visit([NotNull] JToken token);

        // Without this API a string is casted into JValue and doesn't work.
        // Extension does not work either and the JToken overload is picked. 
        [NotNull]
        JToken Visit([NotNull] string json);
    }

//    public static class JsonVisitorExtensions
//    {
//        [NotNull]
//        public static JToken Visit(this IJsonVisitor visitor, [NotNull] string json) => visitor.Visit(JToken.Parse(json));
//    }

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

    /// <summary>
    /// This is an immutable visitor that executes specified visitor in the order they were added.
    /// </summary>
    public class CompositeJsonVisitor : JsonVisitor
    {
        private readonly IImmutableList<IJsonVisitor> _visitors;

        public CompositeJsonVisitor(IEnumerable<IJsonVisitor> visitors) => _visitors = visitors.ToImmutableList();
        
        public static CompositeJsonVisitor Empty => new CompositeJsonVisitor(ImmutableList<IJsonVisitor>.Empty);

        public CompositeJsonVisitor Add(IJsonVisitor visitor) => new CompositeJsonVisitor(_visitors.Add(visitor));

        public override JToken Visit(JToken token) => _visitors.Aggregate(token, (current, visitor) => visitor.Visit(current));
    }

    public class TrimPropertyNameVisitor : JsonVisitor
    {
        protected override JProperty VisitProperty(JProperty property)
        {
            return new JProperty(property.Name.Trim(), Visit(property.Value));
        }
    }

    [PublicAPI]
    public class RewriteTypeVisitor : JsonVisitor
    {
        private readonly string _typePropertyName;

        [NotNull] private readonly ITypeResolver _typeResolver;

        public const string DefaultTypePropertyName = "$type";

        public const string ShortTypePropertyName = "$t";

        public RewriteTypeVisitor([NotNull] string typePropertyName, [NotNull] ITypeResolver typeResolver)
        {
            _typePropertyName = typePropertyName ?? throw new ArgumentNullException(nameof(typePropertyName));
            _typeResolver = typeResolver;
        }

        protected override JProperty VisitProperty(JProperty property)
        {
            return
                SoftString.Comparer.Equals(property.Name, _typePropertyName)
                    ? new JProperty(DefaultTypePropertyName, _typeResolver.Resolve(property.Value.Value<string>()))
                    : base.VisitProperty(property);
        }
    }

    [PublicAPI]
    public class RewritePrettyTypeVisitor : RewriteTypeVisitor
    {
        public RewritePrettyTypeVisitor(IImmutableDictionary<SoftString, Type> types)
            : this(ShortTypePropertyName, types) { }

        public RewritePrettyTypeVisitor(string typePropertyName, IImmutableDictionary<SoftString, Type> types)
            : base(typePropertyName, new PrettyTypeResolver(types)) { }
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