using System;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Reusable.Teapot
{
    public class ContentSection<TContent> where TContent : class
    {
        public ContentSection([NotNull] TContent value, string path = ".")
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Path = path;
        }

        public TContent Value { get; }

        public string Path { get; }
    }

    public static class ContentSection
    {
        // You use ?? to support the null-pattern in case there is no response.

        public static ContentSection<JToken> FromJToken(JToken content, string path = ".")
        {
            return new ContentSection<JToken>(content ?? JToken.Parse("{}"), path);
        }

        public static ContentSection<JValue> FromJValue(JValue content, string path = ".")
        {
            return new ContentSection<JValue>(content ?? JValue.CreateNull(), path);
        }
    }
}