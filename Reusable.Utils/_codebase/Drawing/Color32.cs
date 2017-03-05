using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Reusable.Drawing
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Color32 : IFormattable
    {
        private static readonly ColorParser[] ColorParsers =
        {
            new NameColorParser(),
            new DecimalColorParser(),
            new HexadecimalColorParser()
        };

        [FieldOffset(0)]
        private readonly int _value;

        [FieldOffset(3)]
        private byte _alpha;

        [FieldOffset(2)]
        private byte _red;

        [FieldOffset(1)]
        private byte _green;

        [FieldOffset(0)]
        private byte _blue;

        public Color32(string value)
            : this(Parse(value)) { }

        public Color32(int value)
        {
            _alpha = _red = _green = _blue = 0;
            _value = value;
        }

        public Color32(Color color)
            : this(color.ToArgb())
        { }

        public Color32(byte alpha, byte red, byte green, byte blue)
            : this(Color.FromArgb(alpha, red, green, blue))
        { }

        public Color32(byte red, byte green, byte blue)
            : this(255, red, green, blue)
        { }

        //[JsonProperty("A")]
        public byte Alpha { get { return _alpha; } set { _alpha = value; } }

        //[JsonProperty("R")]
        public byte Red { get { return _red; } set { _red = value; } }

        //[JsonProperty("G")]
        public byte Green { get { return _green; } set { _green = value; } }

        //<[JsonProperty("B")]
        public byte Blue { get { return _blue; } set { _blue = value; } }

        public static Color32 Parse(string value)
        {
            if (TryParse(value, out Color32 color)) return color; else throw new FormatException($"Unknown color format: '{color}'");
        }

        public static bool TryParse(string value, out Color32 color)
        {
            foreach (var colorParser in ColorParsers)
            {
                if (colorParser.TryParse(value, out int result))
                {
                    color = new Color32(result);
                    return true;
                }
            }
            color = new Color32();
            return false;
        }

        public int ToArgb() => ((Color)this).ToArgb();

        public static implicit operator Color32(string value) => Parse(value);

        public static implicit operator Color(Color32 color) => Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);

        public static implicit operator Color32(Color color) => new Color32(color);

        public override bool Equals(object obj) => (obj is Color color && ((Color)this).Equals(obj));

        public override int GetHashCode() => ((Color)this).GetHashCode();

        //public override string ToString() => ToHex();

        public string ToString(string format, IFormatProvider formatProvider) => string.Format(formatProvider, format, this);

        public static bool operator ==(Color32 x, Color32 y) => x._value == y._value;

        public static bool operator !=(Color32 x, Color32 y) => !(x == y);
    }


}