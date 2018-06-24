using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.FormatProviders;

namespace Reusable.Tests.FormatProviders
{
    [TestClass]
    public class HexColorFormatProviderTest
    {
        [TestMethod]
        public void Format_Color_0xARGB()
        {
            var text = string.Format(new HexColorFormatProvider(), "{0:alpha-hex}", Color.DarkRed);

            Assert.AreEqual("FF8B0000", text);
        }
    }
}
