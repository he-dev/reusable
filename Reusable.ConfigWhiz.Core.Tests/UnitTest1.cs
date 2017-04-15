using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Datastores;

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var memory = new MemoryStore
            {
                {"Foo.Bar.Baz", "bar"}
            };
            var configuration = new Configuration(ImmutableList<IDatastore>.Empty.Add(memory));

            var foo = configuration.Resolve<Foo, Bar>();
            ;
        }

        internal class Foo
        {
        }

        internal class Bar
        {
            public string Baz { get; set; }
        }
    }
}
