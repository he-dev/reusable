using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.PropertyMiddleware.Tests
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
}