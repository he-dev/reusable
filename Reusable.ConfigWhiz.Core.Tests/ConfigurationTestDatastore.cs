using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Datastores;
using Reusable.Extensions;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public abstract class ConfigurationTestDatastore
    {
        protected IEnumerable<IDatastore> Datastores { get; set; }

        [TestInitialize]
        public virtual void TestInitialize()
        {

        }

        [TestMethod]
        public void Get_ConsumerWithoutName_GotContainer()
        {
            var memory = new Memory(DatastoreHandle.Default)
            {
                { "Reusable.ConfigWhiz.Tests.Foo.Bar.Baz", "bar" }
            };
            var configuration = new Configuration(ImmutableList<IDatastore>.Empty.Add(memory));

            var foo = configuration.Load<Foo, Bar>();
            foo.Succees.Verify().IsTrue();
            foo.Value.Verify().IsNotNull();
            foo.Value.Baz.Verify().IsEqual("bar");
        }

        internal class Foo { }

        internal class Bar
        {
            public string Baz { get; set; }
        }
    }
}