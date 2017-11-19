using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Experimental;

namespace Reusable.Tests.Experimental
{
    [TestClass]
    public class PropertyReaderTest
    {
        [TestMethod]
        public void GetValue_Property()
        {
            var reader = new PropertyReader<Foo>();
            var foo = new Foo
            {
                Bar = "baz"
            };
            Assert.AreEqual("baz", reader.GetValue<string>(foo, nameof(Foo.Bar)));

            reader.GetValues(foo, new ExpressionList<Foo>
            {
                x => x.Bar
            });
        }

        [TestMethod]
        public void GetValue_Indexer1D()
        {
            var reader = new PropertyReader<Foo>();
            var foo = new Foo
            {
                Bar = "baz"
            };
            Assert.AreEqual("a", reader.GetValue<int, string>(foo, null, 1));
        }

        [TestMethod]
        public void GetValue_Indexer2D()
        {
            var reader = new PropertyReader<Foo>();
            var foo = new Foo
            {
                Bar = "baz"
            };
            Assert.AreEqual("a8", reader.GetValue<int, int, string>(foo, null, 1, 8));
        }

        private class Foo
        {
            public string this[int i] => Bar[i].ToString();
            public string this[int i, int j] => Bar[i].ToString() + j;
            public string Bar { get; set; }
        }
    }

    [TestClass]
    public class PropertyWriterTest
    {
        [TestMethod]
        public void SetValue_Property()
        {
            var reader = new PropertyWriter<Foo>();
            var foo = new Foo
            {
                Bar = "baz"
            };
            reader.SetValue(foo, nameof(Foo.Bar), "qux");

            Assert.AreEqual("qux", foo.Bar);
        }

        [TestMethod]
        public void SetValue_Indexer1D()
        {
            var reader = new PropertyWriter<Foo>();
            var foo = new Foo();

            reader.SetValue(foo, null, 2, 1);
            Assert.AreEqual(2, foo.Ints[1]);
        }

        [TestMethod]
        public void SetValue_Indexer2D()
        {
            var reader = new PropertyWriter<Foo>();
            var foo = new Foo();
            reader.SetValue(foo, null, 2, 1, 2);
            Assert.AreEqual(4, foo.Ints[1]);
        }

        private class Foo
        {
            public int[] Ints { get; } = new int[2];

            public int this[int i]
            {
                get { return Ints[i]; }
                set { Ints[i] = value; }
            }

            public int this[int i, int j]
            {
                get { return Ints[i] + j; }
                set { Ints[i] = value + j; }
            }

            public string Bar { get; set; }
        }
    }
}
