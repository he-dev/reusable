using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Datastores;
using Reusable.Extensions;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
// ReSharper disable InconsistentNaming

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public class ConfigurationTest_General
    {
        [TestMethod]
        public void Get_ConsumerWithoutName_LoadsContainer()
        {
            var memory1 = new Memory("Memory1") { { "Reusable.ConfigWhiz.Tests.Foo.Bar.Qux", "quux" } };
            var memory2 = new Memory("Memory2") { { "Reusable.ConfigWhiz.Tests.Foo.Bar.Baz", "bar" } };

            var configuration = new Configuration(ImmutableList<IDatastore>.Empty.Add(memory1).Add(memory2));

            var foo = configuration.Load<Foo, Bar>();
            foo.Succees.Verify().IsTrue();
            foo.Value.Verify().IsNotNull();
            foo.Value.Baz.Verify().IsEqual("bar");
        }

        [TestMethod]
        public void Get_ConsumerWithName_LoadsContainer()
        {
            var memory2 = new Memory("Memory2") { { @"Reusable.ConfigWhiz.Tests.Foo[""qux""].Bar.Baz", "bar" } };

            var configuration = new Configuration(ImmutableList<IDatastore>.Empty.Add(memory2));

            var foo1 = new Foo { Name = "qux" };
            var foo = configuration.Load<Foo, Bar>(foo1, x => x.Name);
            foo.Succees.Verify().IsTrue();
            foo.Value.Verify().IsNotNull();
            foo.Value.Baz.Verify().IsEqual("bar");
        }

        internal class Foo
        {
            public string Name { get; set; }
        }

        internal class Bar
        {
            public string Baz { get; set; }
        }
    }

    [TestClass]
    public class ConfigurationTest_DatasourceSpecific
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
