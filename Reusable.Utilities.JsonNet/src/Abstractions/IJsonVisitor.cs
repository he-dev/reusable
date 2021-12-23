using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Reusable.Utilities.JsonNet.Abstractions;

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

// public class PropertyNameValidator : JsonVisitor
// {
//     private readonly IImmutableSet<string> _propertyNames;
//
//     public PropertyNameValidator(IEnumerable<string> propertyNames)
//     {
//         _propertyNames = propertyNames.ToImmutableHashSet();
//     }
//
//     protected override JProperty VisitProperty(JProperty property)
//     {
//         return
//             _propertyNames.Contains(property.Name)
//                 ? base.VisitProperty(property)
//                 : throw DynamicException.Create("UnknownPropertyName", $"Property '{property.Name}' is not allowed.");
//     }
// }

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