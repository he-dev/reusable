using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Utilities.JsonNet
{
    public interface ITypeResolver
    {
        [NotNull]
        string Resolve([NotNull] string name);
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
            if (prettyType == null) throw new ArgumentNullException(nameof(prettyType));

            var match =
                Regex
                    .Match(prettyType, PrettyTypePattern, RegexOptions.ExplicitCapture)
                    .OnFailure(_ => new ArgumentException($"Invalid type alias: '{prettyType}'."));

            var generic = GetGenericsInfo(match.Groups["genericArguments"]);
            var type = ResolveType($"{match.Groups["type"].Value}{generic.Placeholder}");
            return $"{type.FullName}{generic.Signature}, {type.Assembly.GetName().Name}";
        }

        // Placeholder: "<, >"
        // Signature: "[[T1],[T2]]" - the generic type count prefix, e.g. "`2" is added by Type.FullName later. 
        private (string Placeholder, string Signature) GetGenericsInfo(Group genericArguments)
        {
            if (genericArguments.Success)
            {
                // "<, >"
                var commas = string.Join(string.Empty, genericArguments.Value.Where(c => c == ',').Select(c => $"{c} "));
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

        [NotNull]
        private Type ResolveType(string typeName)
        {
            try
            {
                return _resolveType(typeName) ?? Type.GetType(typeName, ignoreCase: true, throwOnError: false);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create("TypeNotFound", $"Could not resolve '{typeName}'.", inner);
            }
        }
    }
}