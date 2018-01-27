using System;
using System.Collections.Generic;
using System.Drawing;
using Reusable.Drawing;
using Reusable.Flawless;

namespace Reusable.Converters
{
    public class StringToColorConverter : TypeConverter<String, Color>
    {
        private readonly IEnumerable<ColorParser> _colorParsers;

        public StringToColorConverter() : this(new ColorParser[] { new HexadecimalColorParser() }) { }

        public StringToColorConverter(IEnumerable<ColorParser> colorParsers)
        {
            _colorParsers = colorParsers;
        }

        protected override Color ConvertCore(IConversionContext<String> context)
        {
            foreach (var colorParser in _colorParsers)
            {
                if (colorParser.TryParse(context.Value, out var argb))
                {
                    return new Color32(argb);
                }
            }
            return Color.Empty;
        }
    }

    // ---

    public class ColorToStringConverter : TypeConverter<Color, String>
    {
        private static readonly IValidator<IConversionContext<Color>> ContextValidator= 
            Validator<IConversionContext<Color>>.Empty
                .IsNotValidWhen(x => x.Format == null);

        protected override String ConvertCore(IConversionContext<Color> context)
        {
            context.ValidateWith(ContextValidator).ThrowIfNotValid();

            // ReSharper disable once AssignNullToNotNullAttribute - Validator takes care of context.Format but there is no way to tell it R#
            return string.Format(context.FormatProvider, context.Format, context.Value);
        }
    }
}
