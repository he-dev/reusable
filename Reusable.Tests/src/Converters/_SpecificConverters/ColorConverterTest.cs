using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Drawing;
using Reusable.Formatters;

namespace Reusable.Converters.Tests
{
    [TestClass]
    public class ColorConverterTest
    {
        [TestMethod]
        public void Convert_Name_Color()
        {
            var converter = TypeConverter.Empty.Add(new StringToColorConverter(new[] { new NameColorParser() }));
            var result = converter.Convert(Color.DarkRed.Name, typeof(Color));
            
            Assert.IsTrue(((Color)result).ToArgb() == Color.DarkRed.ToArgb());
        }

        [TestMethod]
        public void Convert_Rgb_Color()
        {
            var converter = TypeConverter.Empty.Add(new StringToColorConverter(new[] { new DecimalColorParser() }));
            var result = converter.Convert($"{Color.Plum.R},{Color.Plum.G},{Color.Plum.B}", typeof(Color));
            
            Assert.IsTrue(((Color)result).ToArgb() == Color.Plum.ToArgb());
        }

        [TestMethod]
        public void Convert_Hex_Color()
        {
            var converter = TypeConverter.Empty.Add(new StringToColorConverter(new[] { new HexadecimalColorParser() }));
            var result = converter.Convert(Color.Beige.ToArgb().ToString("X"), typeof(Color));

            Assert.IsTrue(((Color)result).ToArgb() == Color.Beige.ToArgb());
        }

        [TestMethod]
        public void Convert_Color_Hex()
        {
            var converter = TypeConverter.Empty.Add(new ColorToStringConverter());
            var result = converter.Convert(Color.DeepPink, typeof(String), "{0:0xRGB}", new HexadecimalColorFormatter());

            Assert.IsTrue(((string)result) == "FF1493");
        }
    }
}