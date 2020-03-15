using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Reusable.Utilities.JsonNet.Services;

namespace Reusable.Utilities.JsonNet.Visitors
{
    [PublicAPI]
    public class NormalizePrettyTypeProperty : JsonVisitor
    {
        private readonly INormalizePrettyTypeString _normalizePrettyTypeString;

        public const string DefaultTypePropertyName = "$type";

        public NormalizePrettyTypeProperty(INormalizePrettyTypeString normalizePrettyTypeString)
        {
            _normalizePrettyTypeString = normalizePrettyTypeString;
        }

        public NormalizePrettyTypeProperty(IImmutableDictionary<SoftString, Type> types) : this(new NormalizePrettyTypeString(types)) { }

        protected override JProperty VisitProperty(JProperty property)
        {
            if (ParseTypeName(property) is {} typeName)
            {
                return new JProperty(DefaultTypePropertyName, _normalizePrettyTypeString.Format(typeName));
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
}