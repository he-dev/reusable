using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.FormatProviders;

namespace Reusable.Tests.FormatProviders
{
    [TestClass]
    public class DecimalColorFormatterTest
    {
        [TestMethod]
        public void Format_Color_ARGB()
        {
            var text = string.Format(new RgbColorFormatProvider(), "{0:ARGB}", Color.DarkRed);

            Assert.AreEqual("255, 139, 0, 0", text);
        }
    }
}
