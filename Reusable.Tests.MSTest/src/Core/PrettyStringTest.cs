using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;

namespace Reusable.Tests
{
    [TestClass]
    public class PrettyStringTest
    {
        [TestMethod]
        public void Render_Type_TypeNameWithNamespace()
        {
            var type = typeof(List<IEnumerable<string>>);
            var prettyString = type.ToPrettyString(includeNamespace: true);
            Assert.AreEqual("System.Collections.Generic.List<System.Collections.Generic.IEnumerable<string>>", prettyString);
        }

        [TestMethod]
        public void Render_Type_TypeNameWithoutNamespace()
        {
            var type = typeof(List<IEnumerable<string>>);
            var prettyString = type.ToPrettyString(includeNamespace: false);
            Assert.AreEqual("List<IEnumerable<string>>", prettyString);
        }
    }
}
