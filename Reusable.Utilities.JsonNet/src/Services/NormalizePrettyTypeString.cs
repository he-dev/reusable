using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Utilities.JsonNet.Abstractions;

namespace Reusable.Utilities.JsonNet.Services
{
    [PublicAPI]
    public class NormalizeTypeName : INormalizeTypeName
    {
        public static class TypeNamePatterns
        {
            // language=regexp
            /// <summary>
            /// Specifies the regular c-sharp style type names like: "List<int>" instead of "List`1[System.Int32...]" etc.
            /// </summary>
            /// <remarks>https://regex101.com/r/QZ5T5I/1/</remarks>
            public const string CSharp = @"(?<type>(?i)[a-z0-9_.]+)(?:\<(?<genericArguments>(?i)[a-z0-9_., ]+)\>)?";
        }

        private readonly ITypeDictionary _typeDictionary;

        public NormalizeTypeName(ITypeDictionary typeDictionary)
        {
            _typeDictionary = typeDictionary;
        }

        public string Format(string prettyType)
        {
            var match = ParseTypeName(prettyType, TypeNamePatterns.CSharp);
            var generic = GetGenericsInfo(match.Groups["genericArguments"]);
            var typeName = match.Groups["type"].Value;
            var type = ResolveType($"{typeName}{generic.Placeholder}");
            return $"{type.FullName}{generic.Signature}, {type.Assembly.GetName().Name}";
        }

        private static Match ParseTypeName(string typeName, [RegexPattern] string pattern)
        {
            return
                Regex
                    .Match(typeName, pattern, RegexOptions.ExplicitCapture)
                    .OnFailure(_ => new ArgumentException($"Invalid type name: '{typeName}'. Expected c-sharp-style type name."));
        }

        // Placeholder: "<, >"
        // Signature: "[[T1],[T2]]" - the generic type count prefix, e.g. "`2" is added by Type.FullName later. 
        private (string? Placeholder, string? Signature) GetGenericsInfo(Group genericArguments)
        {
            if (genericArguments.Success)
            {
                // "<, >"
                var commas = genericArguments.Value.Where(c => c == ',').Select(c => $"{c} ").Join(string.Empty);
                var placeholder = $"<{commas}>";

                var genericArgumentNames = genericArguments.Value.Split(',').Select(x => x.Trim()).ToList();
                var genericArgumentFullNames =
                (
                    from name in genericArgumentNames
                    let genericType = ResolveType(name)
                    select $"[{genericType.FullName}, {genericType.Assembly.GetName().Name}]"
                );

                // "[[Namespace.T1, ...],[Namespace.T2, ...]]"
                var signature = $"[{string.Join(",", genericArgumentFullNames)}]";

                return (placeholder, signature);
            }
            else
            {
                return (default, default);
            }
        }

        private Type ResolveType(string typeName)
        {
            return _typeDictionary.Resolve(typeName) ?? throw DynamicException.Create("TypeNotFound", $"Could not resolve '{typeName}'.");
        }
    }
}