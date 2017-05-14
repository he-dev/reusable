using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class UsingifierExtensionsTest
    {
        [TestMethod]
        public void Usingify_NotInitialized_NotDisposed()
        {
            var foo = new Foo();
            using (foo.Usingify(f => f.Bar = "baz")) { }
            Assert.IsNull(foo.Bar);
        }

        [TestMethod]
        public void Usingify_Initialized_Disposed()
        {
            var foo = new Foo();
            using (var usingifier = foo.Usingify(f => f.Bar = "baz")) { usingifier.Initialize(); }
            Assert.AreEqual("baz", foo.Bar);
        }

        class Foo
        {
            public string Bar { get; set; }
        }
    }
}
