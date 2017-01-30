using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Formatters;

namespace Reusable.Tests.Formatters
{
    [TestClass]
    public class DefaultFormatterTest
    {
        [TestMethod]
        public void Format_Int32_String()
        {
            var formatter = CustomFormatter.Default();
            Assert.AreEqual("foo '  32' bar", string.Format(formatter, "foo '{0,4}' bar", 32));
        }

    }
}
