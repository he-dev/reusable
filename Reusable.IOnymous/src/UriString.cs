using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.IOnymous
{
    public class UriString : IEquatable<UriString>, IEquatable<string>
    {
        // https://regex101.com/r/sd288W/1
        // using 'new[]' for _nicer_ syntax
        private static readonly string UriPattern = string.Join(string.Empty, new[]
        {
            /* language=regexp */ @"^(?:(?<scheme>\w+):)?",
            /* language=regexp */ @"(?:\/\/(?<authority>\w+))?",
            /* language=regexp */ @"(?<path>[a-z0-9\/:\.-]+)",
            /* language=regexp */ @"(?:\?(?<query>[a-z0-9=&]+))?",
            /* language=regexp */ @"(?:#(?<fragment>[a-z0-9]+))?"
        });

        private static readonly IEqualityComparer<string> InternalComparer = StringComparer.OrdinalIgnoreCase;

        public static readonly IEqualityComparer<UriString> Comparer = EqualityComparerFactory<UriString>.Create
        (
            @equals: (x, y) =>
            {
                var ignoreScheme = x.IsIOnymous() || y.IsIOnymous();
                return InternalComparer.Equals(ignoreScheme ? x.ToString(string.Empty) : x.ToString(), ignoreScheme ? y.ToString(string.Empty) : y.ToString());
            },
            getHashCode: (obj) => InternalComparer.GetHashCode(obj.IsIOnymous() ? obj.ToString(string.Empty) : obj.ToString()));

        public UriString([NotNull] string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            var uriMatch = Regex.Match
            (
                uri,
                UriPattern,
                RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
            );

            if (!uriMatch.Success)
            {
                throw new ArgumentException(paramName: nameof(uri), message: $"'{uri}' is not a valid Uri.");
            }

            Scheme = uriMatch.Groups["scheme"];
            Authority = uriMatch.Groups["authority"];
            Path = uriMatch.Groups["path"];
            Query =
                uriMatch.Groups["query"].Success
                    ? Regex
                        .Matches
                        (
                            uriMatch.Groups["query"].Value,
                            @"(?:^|&)(?<key>[a-z0-9]+)(?:=(?<value>[a-z0-9]+))?",
                            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
                        )
                        .Cast<Match>()
                        .ToImmutableDictionary
                        (
                            m => (ImplicitString)m.Groups["key"],
                            m => (ImplicitString)m.Groups["value"]
                        )
                    : ImmutableDictionary<ImplicitString, ImplicitString>.Empty;
            Fragment = uriMatch.Groups["fragment"];
        }

        public UriString(UriString absoluteUri, UriString relativeUri)
        {
            if (absoluteUri.IsRelative) throw new ArgumentException($"{nameof(absoluteUri)} must be an absolute Uri.");
            if (relativeUri.IsAbsolute) throw new ArgumentException($"{nameof(relativeUri)} must be a relative Uri.");

            Scheme = absoluteUri.Scheme;
            Authority = absoluteUri.Authority;
            Path = absoluteUri.Path.Value.TrimEnd('/') + "/" + relativeUri.Path.Value.TrimStart('/');
            Query = absoluteUri.Query;
            Fragment = absoluteUri.Fragment;
        }

        public ImplicitString Scheme { get; }

        public ImplicitString Authority { get; }

        public ImplicitString Path { get; }

        public IImmutableDictionary<ImplicitString, ImplicitString> Query { get; }

        public ImplicitString Fragment { get; }

        public bool IsAbsolute => Scheme;

        public bool IsRelative => !IsAbsolute;

        public override string ToString() => ToString(Scheme);

        public string ToString(ImplicitString scheme)
        {
            return string.Join(string.Empty, GetComponents(scheme));
        }

        private IEnumerable<string> GetComponents(ImplicitString scheme)
        {
            if (scheme)
            {
                yield return $"{scheme}:";
            }

            if (Authority)
            {
                yield return $"//{Authority}";
            }

            yield return Path;

            if (Query.Any())
            {
                var queryPairs =
                    Query
                        .OrderBy(x => (string)x.Key, StringComparer.OrdinalIgnoreCase)
                        .Select(x => $"{x.Key}{(x.Value ? "=" : string.Empty)}{x.Value}");
                yield return $"?{string.Join("&", queryPairs)}";
            }

            if (Fragment)
            {
                yield return $"#{Fragment}";
            }
        }

        #region IEquatable

        public bool Equals(UriString other) => Comparer.Equals(this, other);

        public bool Equals(string other) => Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is UriString uri && Equals(uri);

        public override int GetHashCode() => Comparer.GetHashCode(this);

        #endregion

        #region operators

        public static implicit operator UriString(string uri) => new UriString(uri);

        public static implicit operator string(UriString uri) => uri.ToString();

        public static UriString operator +(UriString absoluteUri, UriString relativeUri) => new UriString(absoluteUri, relativeUri);

        #endregion
    }
}