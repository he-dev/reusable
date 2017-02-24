using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Drawing;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.StringFormatting.Formatters;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class ColorConverterTest
    {
        [TestMethod]
        public void Convert_Name_Color()
        {
            var converter = TypeConverter.Empty.Add(new StringToColorConverter(new[] { new NameColorParser() }));
            var result = converter.Convert(Color.DarkRed.Name, typeof(Color));
            result
                .Verify()
                .IsNotNull()
                .IsTrue(x => ((Color)x).ToArgb() == Color.DarkRed.ToArgb());
        }

        [TestMethod]
        public void Convert_Rgb_Color()
        {
            var converter = TypeConverter.Empty.Add(new StringToColorConverter(new[] { new DecimalColorParser() }));
            var result = converter.Convert($"{Color.Plum.R},{Color.Plum.G},{Color.Plum.B}", typeof(Color));
            result
                .Verify()
                .IsNotNull()
                .IsTrue(x => ((Color)x).ToArgb() == Color.Plum.ToArgb());
        }

        [TestMethod]
        public void Convert_Hex_Color()
        {
            var converter = TypeConverter.Empty.Add(new StringToColorConverter(new[] { new HexadecimalColorParser() }));
            var result = converter.Convert(Color.Beige.ToArgb().ToString("X"), typeof(Color));
            result
                .Verify()
                .IsNotNull()
                .IsTrue(x => ((Color)x).ToArgb() == Color.Beige.ToArgb());
        }

        [TestMethod]
        public void Convert_Color_Hex()
        {
            var converter = TypeConverter.Empty.Add(new ColorToStringConverter());
            var result = converter.Convert(Color.DeepPink, typeof(String), "{0:#RRGGBB}", new HexadecimalColorFormatter());
            result
                .Verify()
                .IsNotNull()
                .IsTrue(x => (string)x == "#FF1493");
        }
    }
}