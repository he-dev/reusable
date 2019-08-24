using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Translucent
{
    using System.Linq.Custom;

    internal class PatternAttribute : Attribute
    {
        private readonly string _pattern;

        public PatternAttribute([RegexPattern] string pattern) => _pattern = pattern;

        public override string ToString() => _pattern;
    }

    /// <summary>
    /// Represents: scheme:[//authority]path[?query][#fragment]
    /// </summary>
    [PublicAPI]
    public class UriString : IEquatable<UriString>, IEquatable<string>
    {
        public static readonly IEqualityComparer<UriString> Comparer = EqualityComparerFactory<UriString>.Create
        (
            equals: (x, y) =>
            {
                var equals = (Func<SoftString, SoftString, bool>)SoftString.Comparer.Equals;

                return
                    equals(x.Scheme, y.Scheme) &&
                    equals(x.Authority, y.Authority) &&
                    equals(x.Path.Decoded, y.Path.Decoded) &&
                    x.Query.Count == y.Query.Count && !x.Query.RemoveRange(y.Query.Keys).Any() &&
                    equals(x.Fragment, y.Fragment);
            },
            getHashCode: (obj) => SoftString.Comparer.GetHashCode(obj.ToString()));

        private static readonly Regex Regex;

        static UriString()
        {
            // https://regex101.com/r/sd288W/1

            var patterns =
                from p in typeof(UriString).GetProperties()
                where p.IsDefined(typeof(PatternAttribute))
                select p.GetCustomAttribute<PatternAttribute>();

            Regex = new Regex(patterns.Join(), RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        }
        
        public UriString(SoftString scheme, SoftString authority, string path, IImmutableDictionary<SoftString, SoftString> query, SoftString fragment){}

        public UriString([NotNull] string uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            uri = UriStringHelper.Normalize(uri);

            var uriMatch = Regex.Match(uri);

            if (!uriMatch.Success)
            {
                throw new ArgumentException(paramName: nameof(uri), message: $"'{uri}' is not a valid Uri.");
            }

            Scheme = uriMatch.Groups["scheme"].Value;
            Authority = uriMatch.Groups["authority"].Value;
            Path = new UriStringComponent(UriStringHelper.Encode(uriMatch.Groups["path"].Value.Trim('/')));
            Query =
                uriMatch.Groups["query"].Success
                    ? Regex
                        .Matches
                        (
                            uriMatch.Groups["query"].Value,
                            @"(?:^|&)(?<key>[a-z0-9]+)(?:=(?<value>[a-z0-9\%\.]+))?",
                            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
                        )
                        .Cast<Match>()
                        .ToImmutableDictionary
                        (
                            m => (SoftString)m.Groups["key"].Value,
                            m => (SoftString)m.Groups["value"].Value
                        )
                    : ImmutableDictionary<SoftString, SoftString>.Empty;
            Fragment = uriMatch.Groups["fragment"].Value;
        }

        public UriString(string scheme, string path)
            : this($"{scheme}:{UriStringHelper.Normalize(path)}") { }

        public UriString([NotNull] UriString first, [NotNull] UriString second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            Scheme = first.Scheme;
            Authority = first.Authority;
            Path =
                first.Path.Original
                    ? first.Path.Original.ToString().Trim('/') + "/" + second.Path.Original.ToString().Trim('/')
                    : second.Path.Original.ToString().Trim('/');
            Query = second.Query;
            Fragment = second.Fragment;
        }

        public UriString([NotNull] ImmutableUpdate update)
        {
            update.Bind(this);
        }

        [Pattern(@"^(?:(?<scheme>[a-z0-9\+\.\-]+):)?")]
        public SoftString Scheme { get; }

        [Pattern(@"(?:\/\/(?<authority>[a-z0-9\.\-_:]+))?")]
        public SoftString Authority { get; }

        [Pattern(@"(?:\/?(?<path>[a-z0-9\/:\.\-\%_@]+))")]
        public UriStringComponent Path { get; }

        [Pattern(@"(?:\?(?<query>[a-z0-9=&\%\.]+))?")]
        public IImmutableDictionary<SoftString, SoftString> Query { get; }

        [Pattern(@"(?:#(?<fragment>[a-z0-9]+))?")]
        public SoftString Fragment { get; }

        public bool IsAbsolute => Scheme;

        public bool IsRelative => !IsAbsolute;

        public override string ToString() => ToString(Scheme.ToString());

        public string ToString(SoftString scheme) => string.Join(string.Empty, GetComponents(scheme));

        private IEnumerable<SoftString> GetComponents(SoftString scheme)
        {
            if (scheme)
            {
                yield return $"{scheme.ToString()}:";
            }

            //yield return $"//{Authority.ToString()}{(Path.Original ? "/" : string.Empty)}";

            yield return $"//{(Authority ? Authority.ToString() : string.Empty)}";

            yield return $"/{Path.Original}";

            if (Query.Any())
            {
                var queryPairs =
                    Query
                        .OrderBy(x => x.Key)
                        .Select(x => $"{x.Key.ToString()}{(x.Value ? "=" : string.Empty)}{x.Value.ToString()}");
                yield return $"?{string.Join("&", queryPairs)}";
            }

            if (Fragment)
            {
                yield return $"#{Fragment.ToString()}";
            }
        }

        #region IEquatable

        public bool Equals(UriString other) => Comparer.Equals(this, other);

        public bool Equals(string other) => Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is UriString uri && Equals(uri);

        public override int GetHashCode() => Comparer.GetHashCode(this);

        #endregion

        #region Helpers

        #endregion

        #region operators

        public static implicit operator UriString(string uri) => new UriString(uri);

        //public static implicit operator UriString((string scheme, string path) uri) => new UriString(uri.scheme, uri.path);

        public static implicit operator string(UriString uri) => uri.ToString();

        public static UriString operator +(UriString left, UriString right) => new UriString(left, right);

        public static bool operator ==(UriString left, UriString right) => Comparer.Equals(left, right);

        public static bool operator !=(UriString left, UriString right) => !(left == right);

        #endregion
    }

    public class UriStringComponent
    {
        public UriStringComponent([NotNull] SoftString value) => Original = value ?? throw new ArgumentNullException(nameof(value));

        [NotNull]
        public SoftString Original { get; }

        [NotNull]
        public SoftString Decoded => UriStringHelper.Decode(Original.ToString());

        public static implicit operator UriStringComponent(string value) => new UriStringComponent(value);

        public static implicit operator string(UriStringComponent component) => component.Original.ToString();

        public static implicit operator UriString(UriStringComponent component) => new UriString(component.Original.ToString());
    }
}