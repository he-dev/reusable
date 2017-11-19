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
            var debuggerString = DebuggerString.Create<Person>(new { FirstName = "foo", Age = 19 });
            Assert.AreEqual("Person: FirstName = 'foo' Age = '19'", debuggerString);
        }

        private class Person
        {
            public string FirstName { get; set; }

            public int Age { get; set; }
        }
    }
}
