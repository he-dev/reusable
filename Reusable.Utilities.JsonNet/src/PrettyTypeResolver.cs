using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Utilities.JsonNet
{
    public interface ITypeResolver
    {
        string Resolve(string name);
    }
    
    public class PrettyTypeResolver : ITypeResolver
    {
        // Used to specify user-friendly type names like: "List<int>" instead of "List`1[System.Int32...]" etc.
        // https://regex101.com/r/QZ5T5I/1/
        // language=regexp
        private const string PrettyTypePattern = @"(?<type>(?i)[a-z0-9_.]+)(?:\<(?<genericArguments>(?i)[a-z0-9_., ]+)\>)?";

        private readonly Func<string, Type> _resolveType;
        
        public PrettyTypeResolver([NotNull] IImmutableDictionary<SoftString, Type> types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));
            _resolveType = prettyName => types.TryGetValue(prettyName, out var type) ? type : default;
        }

        public string Resolve(string prettyType)
        {
            var match =
                Regex
                   .Match(prettyType, PrettyTypePattern, RegexOptions.ExplicitCapture)
                   .OnFailure(_ => new ArgumentException($"Invalid type alias: '{prettyType}'."));

            var genericArguments = CreateGenericArguments(match.Groups["genericArguments"]);
            var type = ResolveType($"{match.Groups["type"].Value}{genericArguments.Signature}");
            return $"{type.FullName}{genericArguments.FullName}, {type.Assembly.GetName().Name}";
        }

        // Signature: "<, >"
        // FullName: "[[T1],[T2]]" - the "`2" prefix is provided by type-full-name later 
        private (string Signature, string FullName) CreateGenericArguments(Group genericArguments)
        {
            if (genericArguments.Success)
            {
                // "<, >"
                var commas = string.Join(string.Empty, genericArguments.Value.Where(c => c == ',').Select(c => $"{c} "));
                var signature = $"<{commas}>";

                var genericArgumentNames = genericArguments.Value.Split(',').Select(x => x.Trim()).ToList();
                var genericArgumentFullNames =
                (
                    from name in genericArgumentNames
                    let genericType = ResolveType(name)
                    select $"[{genericType.FullName}, {genericType.Assembly.GetName().Name}]"
                );

                // Creates: "[[Namespace.T1, ...],[Namespace.T2, ...]]"
                var fullName = $"[{string.Join(",", genericArgumentFullNames)}]";

                return (signature, fullName);
            }
            else
            {
                return (default, default);
            }
        }

        private Type ResolveType(string typeName)
        {
            return
                _resolveType(typeName) ??
                Type.GetType(typeName, ignoreCase: true, throwOnError: false) ??
                throw DynamicException.Create("TypeNotFound", $"Could not resolve '{typeName}'.");
        }
    }
}