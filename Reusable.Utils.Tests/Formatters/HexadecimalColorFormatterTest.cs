using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Formatters;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.Tests.Formatters
{
    [TestClass]
    public class HexadecimalColorFormatterTest
    {
        [TestMethod]
        public void Format_Color_AARRGGBB()
        {
            var formatter = CustomFormatter.Default().Add<HexadecimalColorFormatter>();

            var text = string.Format(formatter, "{0:AARRGGBB}", Color.DarkRed);

            var formattedText = "FF8B0000";

            text.Verify().IsEqual(formattedText);
        }
    }
}
