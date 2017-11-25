using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class DisposableExtensionsTest
    {
        [TestMethod]
        public void Usingify_NotInitialized_NotDisposed()
        {
            var foo = new Foo();
            using (foo.ToDisposable(f => f.Bar = "baz")) { }
            Assert.IsNull(foo.Bar);
        }

        [TestMethod]
        public void Usingify_Initialized_Disposed()
        {
            var foo = new Foo();
            using (var usingifier = foo.ToDisposable(f => f.Bar = "baz")) { usingifier.Initialize(); }
            Assert.AreEqual("baz", foo.Bar);
        }

        class Foo
        {
            public string Bar { get; set; }
        }
    }
}
