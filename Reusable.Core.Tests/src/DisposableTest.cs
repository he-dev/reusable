using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class DisposableTest
    {
        //[TestMethod]
        //public void Dispose_NotInitialized_NotDisposed()
        //{
        //    var foo = new Foo();
        //    using (Disposable<Foo>.Create(() => foo, f => f.Bar = "baz"))
        //    {
        //    }
        //    Assert.IsNull(foo.Bar);
        //}

        //[TestMethod]
        //public void Dispose_Initialized_Disposed()
        //{
        //    var foo = new Foo();
        //    using (Disposable<Foo>.CreateInitialized(() => foo, f => f.Bar = "baz"))
        //    {
        //    }
        //    Assert.AreEqual("baz", foo.Bar);
        //}

        class Foo
        {
            public string Bar { get; set; }
        }
    }
}