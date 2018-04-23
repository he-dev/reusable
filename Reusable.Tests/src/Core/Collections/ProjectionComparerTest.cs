using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections;

namespace Reusable.Tests.Collections
{
    [TestClass]
    public class ProjectionComparerTest
    {
        [TestMethod]
        public void Equals_SameValues_True()
        {
            var comparer = ProjectionComparer<Foo>.Create(f => new { f.Bar, f.Baz });
            Assert.IsTrue(comparer.Equals(
                new Foo { Bar = "foo", Baz = 2, Qux = DateTime.Now },
                new Foo { Bar = "foo", Baz = 2, Qux = DateTime.Now.AddHours(-1) })
            );
        }

        [TestMethod]
        public void Equals_DifferentValues_False()
        {
            var comparer = ProjectionComparer<Foo>.Create(f => new { f.Bar, f.Baz });
            Assert.IsFalse(comparer.Equals(
                new Foo { Bar = "foo", Baz = 2, Qux = DateTime.Now },
                new Foo { Bar = "foo", Baz = 3, Qux = DateTime.Now.AddHours(-1) })
            );
        }

        private class Foo
        {
            public string Bar { get; set; }

            public int Baz { get; set; }

            public DateTime Qux { get; set; }
        }
    }
}

