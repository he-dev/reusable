using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class UsingifierTest
    {
        [TestMethod]
        public void Dispose_NotInitialized_NotDisposed()
        {
            var foo = new Foo();
            using (new Usingifier<Foo>(() => foo, f => f.Bar = "baz")) { }
            Assert.IsNull(foo.Bar);
        }

        [TestMethod]
        public void Dispose_Initialized_Disposed()
        {
            var foo = new Foo();
            using (var usingifier = new Usingifier<Foo>(() => foo, f => f.Bar = "baz")) { usingifier.Initialize(); }
            Assert.AreEqual("baz", foo.Bar);
        }

        class Foo
        {
            public string Bar { get; set; }
        }
    }
}
