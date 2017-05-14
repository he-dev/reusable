using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections;

namespace Reusable.Tests.Collections
{
    [TestClass]
    public class AutoKeyDictionaryTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_SameKey_ThrowsArgumentException()
        {
            var dic = ProjectionKeyDictionary<Foo>.Create(f => new { f.Bar, f.Baz });
            dic.Add(new Foo { Bar = "abc", Baz = 2 });
            dic.Add(new Foo { Bar = "abc", Baz = 2 });
        }

        [TestMethod]
        public void Add_DifferentKeys_AddsAll()
        {
            var dic = ProjectionKeyDictionary<Foo>.Create(f => new { f.Bar, f.Baz });
            dic.Add(new Foo { Bar = "abc", Baz = 2 });
            dic.Add(new Foo { Bar = "abc", Baz = 3 });
            Assert.AreEqual(2, dic.Count);
            Assert.IsTrue(dic.ContainsKey(new { Bar = "abc", Baz = 3 }));
            Assert.IsFalse(dic.ContainsKey(new Foo { Bar = "xyz", Baz = 3 }));
        }

        class Foo
        {
            public string Bar { get; set; }

            public int Baz { get; set; }
        }
    }
}
