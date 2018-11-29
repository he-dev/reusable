using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Utilities.JsonNet
{
    public class PrettyTypeExpander
    {
        private static readonly IImmutableDictionary<string, Type> BuiltInTypes =
            ImmutableDictionary
               .Create<string, Type>(StringComparer.OrdinalIgnoreCase)
               .Add("bool", typeof(Boolean))
               .Add("byte", typeof(Byte))
               .Add("sbyte", typeof(SByte))
               .Add("char", typeof(Char))
               .Add("decimal", typeof(Decimal))
               .Add("double", typeof(Double))
               .Add("float", typeof(Single))
               .Add("int", typeof(Int32))
               .Add("uint", typeof(UInt32))
               .Add("long", typeof(Int64))
               .Add("ulong", typeof(UInt64))
               .Add("object", typeof(Object))
               .Add("short", typeof(Int16))
               .Add("ushort", typeof(UInt16))
               .Add("string", typeof(String));

        // Used to specify user-friendly type names like: "List<int>" instead of "List`1[System.Int32...]" etc.
        // https://regex101.com/r/QZ5T5I/1/
        // language=regexp
        private const string PrettyTypePattern = @"(?<type>(?i)[a-z0-9_.]+)(?:\<(?<genericArguments>(?i)[a-z0-9_., ]+)\>)?";

        private readonly Func<string, Type> _resolveCustomType;

        public PrettyTypeExpander([NotNull] Func<string, Type> resolveCustomType)
        {
            _resolveCustomType = resolveCustomType;
        }

        public string Expand(string prettyType)
        {
            var match =
                Regex
                   .Match(prettyType, PrettyTypePattern, RegexOptions.ExplicitCapture)
                   .OnFailure(_ => new ArgumentException($"Invalid type alias: '{prettyType}'."));

            var genericArguments = CreateGenericArguments(match.Groups["genericArguments"]);
            var type = ResolveCustomType($"{match.Groups["type"].Value}{genericArguments.Signature}");
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
                    let genericType = ResolveBuiltInType(name) ?? ResolveCustomType(name)
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

        [CanBeNull]
        private static Type ResolveBuiltInType(string name) => BuiltInTypes.TryGetValue(name, out var t) ? t : default;

        private Type ResolveCustomType(string typeName)
        {
            return
                _resolveCustomType(typeName) ??
                Type.GetType(typeName, ignoreCase: true, throwOnError: false) ??
                throw DynamicException.Create("TypeNotFound", $"Could not resolve '{typeName}'.");
        }
    }
}