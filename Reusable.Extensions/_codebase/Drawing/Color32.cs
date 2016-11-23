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
            Color32 color;
            if (!TryParse(value, out color))
            {
                throw new FormatException($"Unknown color format: '{color}'");
            }
            return color;
        }

        public static bool TryParse(string value, out Color32 color)
        {
            foreach (var colorParser in ColorParsers)
            {
                var result = 0;
                if (colorParser.TryParse(value, out result))
                {
                    color = new Color32(result);
                    return true;
                }
            }
            color = new Color32();
            return false;
        }

        public static implicit operator Color32(string value)
        {
            return Parse(value);
        }

        public static implicit operator Color(Color32 color)
        {
            return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        }

        public static implicit operator Color32(Color color)
        {
            return new Color32(color);
        }

        public override bool Equals(object obj)
        {
            return obj != null && ((Color)this).Equals(obj);
        }

        public override int GetHashCode()
        {
            return ((Color)this).GetHashCode();
        }

        public override string ToString()
        {
            return ToHex();
        }

        public int ToArgb()
        {
            return _value;
        }

        public string ToDec()
        {
            const string separator = ",";

            return new StringBuilder()
                .Append(Alpha != 0xFF ? Alpha + separator : string.Empty)
                .Append(Red).Append(separator)
                .Append(Green).Append(separator)
                .Append(Blue)
                .ToString();
        }

        public string ToHex()
        {
            const string hexFormat = "X2";

            var color = new StringBuilder()
                .Append("#")
                .Append(Alpha != 0xFF ? Alpha.ToString(hexFormat) : string.Empty)
                .Append(Red.ToString(hexFormat))
                .Append(Green.ToString(hexFormat))
                .Append(Blue.ToString(hexFormat))
                .ToString();
            return color;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return null;
        }

        public static bool operator ==(Color32 x, Color32 y)
        {
            return x._value == y._value;
        }

        public static bool operator !=(Color32 x, Color32 y)
        {
            return !(x == y);
        }
    }


}