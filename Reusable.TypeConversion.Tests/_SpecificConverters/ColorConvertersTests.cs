using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Converters.Converters;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class ColorConvertersTests : ConverterTest
    {
        [TestMethod]
        public void ConvertStringToColor()
        {
            Convert<StringToColorConverter>(Color.DarkRed.Name, typeof(Color))
                .Verify()
                .IsNotNull()
                .IsTrue(x => ((Color)x).ToArgb() == Color.DarkRed.ToArgb());

            Convert<StringToColorConverter>($"{Color.Plum.R},{Color.Plum.G},{Color.Plum.B}", typeof(Color))
                .Verify()
                .IsNotNull()
                .IsTrue(x => ((Color)x).ToArgb() == Color.Plum.ToArgb());

            Convert<StringToColorConverter>(Color.Beige.ToArgb().ToString("X"), typeof(Color))
                .Verify()
                .IsNotNull()
                .IsTrue(x => ((Color)x).ToArgb() == Color.Beige.ToArgb());
        }

        [TestMethod]
        public void ConvertColorToString()
        {
            Convert<ColorToStringConverter>(Color.DeepPink, typeof(String))
                .Verify()
                .IsNotNull()
                .IsTrue(x => (string)x == "#FF1493");
        }
    }
}