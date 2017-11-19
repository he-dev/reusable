using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Drawing;
using Reusable.Extensions;
using Reusable.Formatters;

namespace Reusable.ThirdParty.JsonNetUtilities
{
    [PublicAPI]
    public class JsonColorConverter : JsonConverter
    {
        [NotNull]
        private IEnumerable<ColorParser> _colorParsers;

        [NotNull]
        private ICustomFormatter _colorFormatter;

        [NotNull]
        private string _colorFormat;

        public JsonColorConverter()
        {
            _colorParsers = new ColorParser[]
            {
                new DecimalColorParser(),
                new HexadecimalColorParser(),
                new NameColorParser(),
            };

            _colorFormatter = new HexadecimalColorFormatter();

            _colorFormat = "#RGB";
        }

        [NotNull, ItemNotNull]
        public IEnumerable<ColorParser> ColorParsers
        {
            get => _colorParsers;
            set => _colorParsers = value ?? throw new ArgumentNullException(nameof(ColorParsers));
        }

        [NotNull]
        public ICustomFormatter ColorFormatter
        {
            get => _colorFormatter;
            set => _colorFormatter = value ?? throw new ArgumentNullException(nameof(ColorFormatter));
        }

        [NotNull]
        public string ColorFormat
        {
            get => _colorFormat;
            set => _colorFormat = value ?? throw new ArgumentNullException(nameof(ColorFormat));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jToken = JToken.Load(reader);
            var colorString = jToken.Value<string>();

            foreach (var colorParser in ColorParsers)
            {
                if (colorParser.TryParse(colorString, out var color))
                {
                    return Color.FromArgb(color);
                }
            }
            throw new JsonSerializationException($"Unrecognized color format: '{colorString}'");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var colorString = ColorFormatter.Format(ColorFormat, (Color)value, CultureInfo.InvariantCulture);
            if (colorString.IsNullOrEmpty())
            {
                throw new JsonSerializationException($"Unrecognized color format: '{ColorFormat}'");
            }
            writer.WriteValue(colorString);
        }
    }
}