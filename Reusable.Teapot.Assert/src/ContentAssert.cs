using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Reusable.Marbles;

namespace Reusable.Teapot
{
    public static class ContentAssert
    {
        // Use this the same pattern for each assert: condition ? throw : content

        #region JToken helpers 

        [NotNull]
        public static ContentSection<JValue> Value(this ContentSection<JToken> content, string jsonPath)
        {
            return
                !(content.Value.SelectToken(jsonPath) is JValue value)
                    ? throw DynamicException.Create("ContentPropertyNotFound", $"There is no such property as '{jsonPath}'")
                    : ContentSection.FromJValue(value, jsonPath);
        }

        [NotNull]
        public static ContentSection<JToken> HasProperty(this ContentSection<JToken> content, string jsonPath)
        {
            return
                content.Value.SelectToken(jsonPath) is null
                    ? throw DynamicException.Create("ContentPropertyNotFound", $"There is no such property as '{jsonPath}'")
                    : content;
        }

        #endregion

        #region JValue helpers

        [NotNull]
        public static ContentSection<JValue> IsNotNull(this ContentSection<JValue> content)
        {
            return
                content.Value.Equals(JValue.CreateNull())
                    ? throw DynamicException.Create("ValueNull", $"Value at '{content.Path}' is null.")
                    : content;
        }

        [NotNull]
        public static ContentSection<JValue> IsEqual(this ContentSection<JValue> content, object expected)
        {
            return
                !content.Value.Equals(new JValue(expected))
                    ? throw DynamicException.Create("ValueNotEqual", $"Value at '{content.Path}' is '{content.Value}' but should be '{expected}'.")
                    : content;
        }

        [NotNull]
        public static ContentSection<JValue> IsLike(this ContentSection<JValue> content, [RegexPattern] string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return
                !Regex.IsMatch(content.Value.Value<string>() ?? string.Empty, pattern, options)
                    ? throw DynamicException.Create("ValueNotLike", $"Value at '{content.Path}' is '{content.Value}' but should be like '{pattern}'.")
                    : content;
        }

        #endregion
    }
}