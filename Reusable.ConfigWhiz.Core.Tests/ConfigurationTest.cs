using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Datastores;
using Reusable.Extensions;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public class ConfigurationTest
    {
        [TestMethod]
        public void Get1()
        {
            var memory = new Memory(DatastoreHandle.Default)
            {
                { "Reusable.ConfigWhiz.Tests.Foo.Bar.Baz", "bar" }
            };
            var configuration = new Configuration(ImmutableList<IDatastore>.Empty.Add(memory));

            var foo = configuration.Load<Foo, Bar>();
            foo.Verify().IsNotNull();
            foo.Baz.Verify().IsEqual("bar");
        }

        internal class Foo { }

        internal class Bar
        {
            public string Baz { get; set; }
        }
    }
}
