using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Drawing;

namespace Reusable.Tests.Drawing
{
    [TestCategory("Colors")]
    public abstract class ColorParserTest
    {
        protected ColorParserTest(ColorParser parser) => Parser = parser;

        private ColorParser Parser { get; }

        [TestMethod]
        public void Parse_Null_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Parser.Parse(null));
        }

        [TestMethod]
        public void Parse_NotColor_Throws()
        {
            Assert.ThrowsException<FormatException>(() => Parser.Parse("abc"));
        }

        [TestMethod]
        public void Parse_CadetBlue_Color()
        {
            Assert.AreEqual(Color.CadetBlue.ToArgb(), Parser.Parse(GetCadetBlue()));
        }

        [TestMethod]
        public void TryParse_Null_False()
        {
            Assert.IsFalse(Parser.TryParse(null, out var color));
        }

        [TestMethod]
        public void TryParse_Color_True()
        {
            Assert.IsTrue(Parser.TryParse(GetCadetBlue(), out var color));
            Assert.AreEqual(Color.CadetBlue.ToArgb(), color);
        }

        protected abstract string GetCadetBlue();

    }

    [TestClass]
    public class DecimalColorParserTest : ColorParserTest
    {
        public DecimalColorParserTest() : base(new DecimalColorParser()) { }

        protected override string GetCadetBlue() => "95,158,160";
    }

    [TestClass]
    public class HexadecimalColorParserTest : ColorParserTest
    {
        public HexadecimalColorParserTest() : base(new HexadecimalColorParser()) { }

        protected override string GetCadetBlue() => "0x5F9EA0";
    }

    [TestClass]
    public class NameColorParserTest : ColorParserTest
    {
        public NameColorParserTest() : base(new NameColorParser()) { }

        protected override string GetCadetBlue() => "CadetBlue";
    }
}
