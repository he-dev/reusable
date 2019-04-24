using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Diagnostics;

namespace Reusable.Tests.Diagnostics
{
    [TestClass]
    public class DebuggerStringTest
    {
        [TestMethod]
        public void Create_TypeAndObject_String()
        {
            var debuggerString = new Person { FirstName = "foo", Age = 19 }.ToDebuggerDisplayString(builder => builder.DisplayValue(x => x.FirstName).DisplayValue(x => x.Age));
            Assert.AreEqual("FirstName = 'foo', Age = 19", debuggerString);
        }

        private class Person
        {
            public string FirstName { get; set; }

            public int Age { get; set; }
        }
    }
}
