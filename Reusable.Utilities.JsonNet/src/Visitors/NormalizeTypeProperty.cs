using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Reusable.Essentials.Extensions;
using Reusable.Utilities.JsonNet.Abstractions;
using Reusable.Utilities.JsonNet.Services;

namespace Reusable.Utilities.JsonNet.Visitors;

[PublicAPI]
public class NormalizeTypeProperty : JsonVisitor
{
    private readonly INormalizeTypeName _normalizeTypeName;

    public const string DefaultTypePropertyName = "$type";

    public NormalizeTypeProperty(INormalizeTypeName normalizeTypeName)
    {
        _normalizeTypeName = normalizeTypeName;
    }

    public NormalizeTypeProperty(ITypeDictionary typeDictionary) : this(new NormalizeTypeName(typeDictionary)) { }

    protected override JProperty VisitProperty(JProperty property)
    {
        if (!property.Name.Matches(@"^\$type") && ParseTypeName(property) is {} typeName)
        {
            return new JProperty(DefaultTypePropertyName, _normalizeTypeName.Format(typeName));
        }
        else
        {
            return base.VisitProperty(property);
        }
    }

    private string? ParseTypeName(JProperty property)
    {
        var typeMatch = Regex.Match(property.Name.Trim(), @"^\$(?<Schema>[a-z0-9_-]+)", RegexOptions.IgnoreCase);
        return typeMatch.Success ? FormatTypeName(typeMatch.Groups["Schema"].Value, property.Value.Value<string>()) : default;
    }

    private string FormatTypeName(string schema, string typeName)
    {
        return
            typeName.Contains('.')
                ? typeName
                : $"{schema}.{typeName}";
    }
}