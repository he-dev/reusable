using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Reusable.Diagnostics;

namespace Reusable.Net.Http
{
    [PublicAPI]
    public class PartialUriBuilder
    {
        public PartialUriBuilder() { }

        public PartialUriBuilder(params string[] path)
        {
            Path.AddRange(path);
        }

        private string DebuggerDispaly => ToString();

        [NotNull]
        public UriPath Path { get; } = new UriPath();

        [NotNull]
        public UriQuery Query { get; } = new UriQuery();

        public override string ToString() => $"{Path}{Query}";

        [NotNull]
        public Uri ToUri(Uri baseAddress) => new Uri(baseAddress, ToString());

        public static implicit operator string(PartialUriBuilder builder) => builder?.ToString();
    }

    public class UriPath : List<string>
    {
        public override string ToString() => string.Join("/", this.Select(HttpUtility.UrlEncode));

        public static implicit operator string(UriPath builder) => builder?.ToString();
    }

    public class UriQuery : List<(string Key, string Value)>
    {
        public void Add(string key, string value) => Add((key, value));

        public override string ToString() => this.Any() ? "?" + string.Join("&", this.Select(x => $"{x.Key}={x.Value}")) : string.Empty;

        public static implicit operator string(UriQuery builder) => builder?.ToString();
    }
}