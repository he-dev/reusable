using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.FormatProviders;

namespace Reusable.Tests.FormatProviders
{
    using static FormattableStringHelper;

    [TestClass]
    public class TypeFormatProviderTest
    {
        [TestMethod]
        public void CanFormatWithOrWithoutNamespace()
        {
            var formatter = new TypeFormatProvider();

            Assert.AreEqual("List<int>", Format($"{typeof(List<int>)}", formatter));
            Assert.AreEqual("System.Collections.Generic.List<int>", Format($"{typeof(List<int>):wns}", formatter));
        }
    }
}