using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Formatters;

namespace Reusable.Tests.Formatters
{
    [TestClass]
    public class HexadecimalColorFormatterTest
    {
        [TestMethod]
        public void Format_Color_0xARGB()
        {
            var formatter = CustomFormatter.Default().Add<HexadecimalColorFormatter>();

            var text = string.Format(formatter, "{0:0xARGB}", Color.DarkRed);

            Assert.AreEqual("FF8B0000", text);
        }
    }
}
