using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;

namespace Reusable.Net.Http
{
    [PublicAPI]
    public class UriDynamicPart
    {
        public UriDynamicPart() { }

        public UriDynamicPart(params string[] path)
        {
            Path.AddRange(path);
        }

        public UriPath Path { get; set; } = new UriPath();

        public UriQuery Query { get; set; } = new UriQuery();

        public override string ToString()
        {
            return $"{Path}{Query}";
        }

        public static implicit operator string(UriDynamicPart builder) => builder?.ToString();
    }

    public class UriPath : List<string>
    {
        public override string ToString()
        {
            return string.Join("/", this.Select(HttpUtility.UrlEncode));
        }

        public static implicit operator string(UriPath builder) => builder?.ToString();
    }

    public class UriQuery : List<(string Key, string Value)>
    {
        public void Add(string key, string value)
        {
            Add((key, value));
        }

        public override string ToString()
        {
            return this.Any() ? "?" + string.Join("&", this.Select(x => $"{x.Key}={x.Value}")) : string.Empty;
        }

        public static implicit operator string(UriQuery builder) => builder?.ToString();
    }
}