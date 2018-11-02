using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Convertia;
using Reusable.Convertia.Converters;
using Reusable.Drawing;
using Reusable.FormatProviders;

namespace Reusable.Tests.Converters
{
    [TestClass]
    public class ColorTest
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
            var converter = TypeConverter.Empty.Add(new StringToColorConverter(new[] { new RgbColorParser() }));
            var result = converter.Convert($"{Color.Plum.R},{Color.Plum.G},{Color.Plum.B}", typeof(Color));
            
            Assert.IsTrue(((Color)result).ToArgb() == Color.Plum.ToArgb());
        }

        [TestMethod]
        public void Convert_Hex_Color()
        {
            var converter = TypeConverter.Empty.Add(new StringToColorConverter(new[] { new HexColorParser() }));
            var result = converter.Convert(Color.Beige.ToArgb().ToString("X"), typeof(Color));

            Assert.IsTrue(((Color)result).ToArgb() == Color.Beige.ToArgb());
        }

        [TestMethod]
        public void Convert_Color_Hex()
        {
            var converter = TypeConverter.Empty.Add(new ColorToStringConverter());
            var result = converter.Convert(Color.DeepPink, typeof(String), "#{0:hex}", new HexColorFormatProvider());

            Assert.AreEqual("#FF1493", result.ToString());
        }
    }
}