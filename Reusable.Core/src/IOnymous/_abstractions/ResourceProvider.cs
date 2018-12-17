using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IResourceProvider
    {
        [NotNull]
        ResourceProviderMetadata Metadata { get; }

        [ItemNotNull]
        Task<IResourceInfo> GetAsync([NotNull] SimpleUri uri, ResourceProviderMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> PostAsync([NotNull] SimpleUri uri, [NotNull] Stream value, ResourceProviderMetadata metadata = null);

        //[ItemNotNull]
        //Task<IResourceInfo> PostAsync([NotNull] SimpleUri uri, [NotNull] object value, ResourceProviderMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> PutAsync([NotNull] SimpleUri uri, [NotNull] Stream value, ResourceProviderMetadata metadata = null);

        //[ItemNotNull]
        //Task<IResourceInfo> PutAsync([NotNull] SimpleUri uri, [NotNull] object value, ResourceProviderMetadata metadata = null);

        [ItemNotNull]
        Task<IResourceInfo> DeleteAsync([NotNull] SimpleUri uri, ResourceProviderMetadata metadata = null);
    }

    public abstract class ResourceProvider : IResourceProvider
    {
        public static readonly string Scheme = "ionymous";

        protected ResourceProvider(ResourceProviderMetadata metadata)
        {
            Metadata = metadata;
        }

        public virtual ResourceProviderMetadata Metadata { get; }

        public abstract Task<IResourceInfo> GetAsync(SimpleUri uri, ResourceProviderMetadata metadata = null);

        public virtual Task<IResourceInfo> PostAsync(SimpleUri name, Stream value, ResourceProviderMetadata metadata = null) { throw new NotImplementedException(); }

        //public virtual Task<IResourceInfo> PostAsync(SimpleUri name, object value, ResourceProviderMetadata metadata = null) { throw new NotImplementedException(); }

        public abstract Task<IResourceInfo> PutAsync(SimpleUri uri, Stream value, ResourceProviderMetadata metadata = null);

        //public abstract Task<IResourceInfo> PutAsync(SimpleUri uri, object value, ResourceProviderMetadata metadata = null);

        public abstract Task<IResourceInfo> DeleteAsync(SimpleUri uri, ResourceProviderMetadata metadata = null);

        protected static Exception CreateException(IResourceProvider provider, string name, ResourceProviderMetadata metadata, Exception inner, [CallerMemberName] string memberName = null)
        {
            return new Exception();
        }
    }

    public static class ResourceProviderExtensions
    {
        public static async Task<IResourceInfo> PutAsync(this IResourceProvider resourceProvider, SimpleUri uri, object value, ResourceProviderMetadata metadata = null)
        {
            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, value);
                return await resourceProvider.PutAsync(uri, memoryStream, (metadata ?? ResourceProviderMetadata.Empty).Add("StreamType", "Object"));
            }
        }
    }

    public class ResourceProviderMetadata
    {
        private readonly IImmutableDictionary<SoftString, object> _metadata;

        public ResourceProviderMetadata() : this(ImmutableDictionary<SoftString, object>.Empty) { }

        private ResourceProviderMetadata(IImmutableDictionary<SoftString, object> metadata) => _metadata = metadata;

        public static ResourceProviderMetadata Empty => new ResourceProviderMetadata();

        public object this[SoftString key] => _metadata[key];
        public int Count => _metadata.Count;
        public IEnumerable<SoftString> Keys => _metadata.Keys;
        public IEnumerable<object> Values => _metadata.Values;
        public bool ContainsKey(SoftString key) => _metadata.ContainsKey(key);
        public bool Contains(KeyValuePair<SoftString, object> pair) => _metadata.Contains(pair);
        public bool TryGetKey(SoftString equalKey, out SoftString actualKey) => _metadata.TryGetKey(equalKey, out actualKey);
        public bool TryGetValue(SoftString key, out object value) => _metadata.TryGetValue(key, out value);

        public ResourceProviderMetadata Add(SoftString key, object value) => new ResourceProviderMetadata(_metadata.Add(key, value));
        //public IImmutableDictionary<SoftString, object> Clear() => new ResourceProviderMetadata(_metadata.Clear());
        //public IImmutableDictionary<SoftString, object> AddRange(IEnumerable<KeyValuePair<SoftString, object>> pairs) => new ResourceProviderMetadata(_metadata.AddRange(pairs));
        //public IImmutableDictionary<SoftString, object> SetItem(SoftString key, object value) => new ResourceProviderMetadata(_metadata.SetItem(key, value));
        //public IImmutableDictionary<SoftString, object> SetItems(IEnumerable<KeyValuePair<SoftString, object>> items) => new ResourceProviderMetadata(_metadata.SetItems(items));
        //public IImmutableDictionary<SoftString, object> RemoveRange(IEnumerable<SoftString> keys) => new ResourceProviderMetadata(_metadata.RemoveRange(keys));
        //public IImmutableDictionary<SoftString, object> Remove(SoftString key) => new ResourceProviderMetadata(_metadata.Remove(key));

        //public IEnumerator<KeyValuePair<SoftString, object>> GetEnumerator() => _metadata.GetEnumerator();
        //IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_metadata).GetEnumerator();
    }

    public static class ResourceProviderMetadataExtensions
    {
        public static bool TryGetValue<T>(this ResourceProviderMetadata metadata, SoftString key, out T value)
        {
            if (!(metadata is null) && metadata.TryGetValue(key, out var x) && x is T result)
            {
                value = result;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }

    public static class ResourceProviderMetadataKeyNames
    {
        public static string ProviderName { get; } = nameof(ProviderName);

        public static string CanGet { get; } = nameof(CanGet);

        public static string CanPost { get; } = nameof(CanPost);

        public static string CanPut { get; } = nameof(CanPut);

        public static string CanDelete { get; } = nameof(CanDelete);

        //public static string CanDeserialize { get; } = nameof(CanDeserialize);
    }


    public class SimpleUri : IEquatable<SimpleUri>, IEquatable<string>
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

        public static readonly IEqualityComparer<SimpleUri> Comparer = EqualityComparerFactory<SimpleUri>.Create
        (
            equals: (x, y) =>
            {
                var ignoreScheme = x.IsIOnymous() || y.IsIOnymous();
                return InternalComparer.Equals(ignoreScheme ? x.ToString(string.Empty) : x.ToString(), ignoreScheme ? y.ToString(string.Empty) : y.ToString());
            },
            getHashCode: (obj) => InternalComparer.GetHashCode(obj.IsIOnymous() ? obj.ToString(string.Empty) : obj.ToString()));

        public SimpleUri([NotNull] string uri)
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

        public SimpleUri(SimpleUri absoluteUri, SimpleUri relativeUri)
        {
            if (absoluteUri.IsRelative) throw new ArgumentException($"{nameof(absoluteUri)} must be an absolute Uri.");
            if (!relativeUri.IsRelative) throw new ArgumentException($"{nameof(relativeUri)} must be a relative Uri.");

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

        public bool Equals(SimpleUri other) => Comparer.Equals(this, other);

        public bool Equals(string other) => Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is SimpleUri uri && Equals(uri);

        public override int GetHashCode() => Comparer.GetHashCode(this);

        #endregion

        #region operators

        public static implicit operator SimpleUri(string uri) => new SimpleUri(uri);

        public static implicit operator string(SimpleUri uri) => uri.ToString();

        public static SimpleUri operator +(SimpleUri absoluteUri, SimpleUri relativeUri) => new SimpleUri(absoluteUri, relativeUri);

        #endregion
    }

    public class ImplicitString : IEquatable<ImplicitString>
    {
        public ImplicitString(string value) => Value = value;

        [AutoEqualityProperty]
        public string Value { get; }

        public override string ToString() => Value;

        public static implicit operator ImplicitString(string value) => new ImplicitString(value);

        public static implicit operator ImplicitString(Group group) => group.Value;

        public static implicit operator string(ImplicitString value) => value.ToString();

        public static implicit operator bool(ImplicitString value) => !string.IsNullOrWhiteSpace(value);

        #region IEquatable

        public bool Equals(ImplicitString other) => AutoEquality<ImplicitString>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is ImplicitString str && Equals(str);

        public override int GetHashCode() => AutoEquality<ImplicitString>.Comparer.GetHashCode(this);

        #endregion
    }
}
