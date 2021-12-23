using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Essentials.Drawing;
using Reusable.Essentials.FormatProviders;

namespace Reusable.Utilities.JsonNet.Converters;

[PublicAPI]
public class ColorConverter : JsonConverter
{
    private IEnumerable<ColorParser> _colorParsers;

    private string _colorFormat;

    private IFormatProvider _colorFormatProvider;

    public ColorConverter()
    {
        _colorParsers = new ColorParser[]
        {
            new RgbColorParser(),
            new HexColorParser(),
            new NameColorParser(),
        };

        _colorFormat = "{0:hex}";
        _colorFormatProvider = new HexColorFormatProvider();
    }

    public IEnumerable<ColorParser> ColorParsers
    {
        get => _colorParsers;
        set => _colorParsers = value ?? throw new ArgumentNullException(nameof(ColorParsers));
    }

    public string ColorFormat
    {
        get => _colorFormat;
        set => _colorFormat = value ?? throw new ArgumentNullException(nameof(ColorFormat));
    }

    public IFormatProvider ColorFormatProvider
    {
        get => _colorFormatProvider;
        set => _colorFormatProvider = value ?? throw new ArgumentNullException(nameof(ColorFormatProvider));
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Color);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
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

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        var hexColorString = string.Format(ColorFormatProvider, $"#{ColorFormat}", value); // ColorFormatProvider.Format(ColorFormat, (Color)value, CultureInfo.InvariantCulture);
        writer.WriteValue(hexColorString);
    }
}