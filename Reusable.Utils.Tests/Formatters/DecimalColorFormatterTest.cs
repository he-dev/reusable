using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.StringFormatting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.StringFormatting.Formatters;

namespace Reusable.Tests.Formatters
{
    [TestClass]
    public class DecimalColorFormatterTest
    {
        [TestMethod]
        public void Format_Color_ARGB()
        {
            var formatter = CustomFormatter.Default().Add<DecimalColorFormatter>();

            var text = string.Format(formatter, "{0:ARGB}", Color.DarkRed);

            var formattedText = "255, 139, 0, 0";

            text.Verify().IsEqual(formattedText);
        }
    }
}
