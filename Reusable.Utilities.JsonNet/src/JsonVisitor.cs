using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionize;

namespace Reusable.Utilities.JsonNet
{
    public interface IJsonVisitor
    {
        JToken Visit(JToken token);

        // Without this API a string is casted into JValue and doesn't work.
        // Extension does not work either and the JToken overload is picked. 
        JToken Visit(string json);
    }

    public abstract class JsonVisitor : IJsonVisitor
    {
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
            return tokenType switch
            {
                JTokenType.Object => new JObject(),
                JTokenType.Array => new JArray(),
                _ => throw new ArgumentOutOfRangeException
                (
                    paramName: nameof(tokenType),
                    message: $"Invalid token type: {tokenType} (supported are only [{string.Join(", ", new[] { JTokenType.Object, JTokenType.Array })}]"
                )
            };
        }
    }

    public static class JsonVisitorExtensions
    {
        public static JToken Visit(this IEnumerable<IJsonVisitor> visitors, JToken token)
        {
            return visitors.Aggregate(token, (current, visitor) => visitor.Visit(current));
        }
    }

    /// <summary>
    /// This is an immutable visitor that executes specified visitor in the order they were added.
    /// </summary>
    public class CompositeJsonVisitor : JsonVisitor, IEnumerable<IJsonVisitor>
    {
        private readonly IList<IJsonVisitor> _visitors = new List<IJsonVisitor>();

        //public CompositeJsonVisitor(IEnumerable<IJsonVisitor> visitors) => _visitors = visitors.ToImmutableList();

        //public static CompositeJsonVisitor Empty => new CompositeJsonVisitor(ImmutableList<IJsonVisitor>.Empty);

        public void Add(IJsonVisitor visitor) => _visitors.Add(visitor);

        public override JToken Visit(JToken token) => this.Aggregate(token, (current, visitor) => visitor.Visit(current));
        
        public IEnumerator<IJsonVisitor> GetEnumerator() => _visitors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_visitors).GetEnumerator();
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
        private readonly ITypeResolver _typeResolver;

        public const string DefaultTypePropertyName = "$type";

        public RewriteTypeVisitor(ITypeResolver typeResolver)
        {
            _typeResolver = typeResolver;
        }

        protected override JProperty VisitProperty(JProperty property)
        {
            if (ParseTypeName(property) is {} typeName)
            {
                return new JProperty(DefaultTypePropertyName, _typeResolver.Resolve(typeName));
            }
            else
            {
                return base.VisitProperty(property);
            }
        }

        private string? ParseTypeName(JProperty property)
        {
            var typeMatch = Regex.Match(property.Name.Trim(), @"^\$(?<Namespace>[a-z0-9_-]+)", RegexOptions.IgnoreCase);
            return typeMatch.Success ? FormatTypeName(typeMatch.Groups["Namespace"].Value, property.Value.Value<string>()) : default;
        }

        private string FormatTypeName(string @namespace, string typeName)
        {
            return
                typeName.Contains('.')
                    ? typeName
                    : $"{@namespace}.{typeName}";
        }
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