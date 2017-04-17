using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Core.Tests.Data;
using Reusable.ConfigWhiz.Datastores;
using Reusable.Extensions;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
// ReSharper disable InconsistentNaming

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public class ConfigurationTestCore : ConfigurationTestDatastore
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Datastores = new IDatastore[]
            {
                new Memory("Memory1")
                {
                    { @"Reusable.ConfigWhiz.Tests.Foo.Bar.Qux", "quux" }
                },
                new Memory("Memory2")
                {
                    { @"Reusable.ConfigWhiz.Tests.Foo.Bar.Baz", "bar" },
                    { @"Reusable.ConfigWhiz.Tests.Foo[""qux""].Bar.Baz", "bar" }
                }
            };
        }

        [TestMethod]
        public void Load_ConsumerWithoutName_Success()
        {
            var configuration = new Configuration(Datastores);

            var foo = configuration.Load<Foo, Bar>();
            foo.Succees.Verify().IsTrue();
            foo.Value.Verify().IsNotNull();
            foo.Value.Baz.Verify().IsEqual("bar");
        }

        [TestMethod]
        public void Load_ConsumerWithName_Success()
        {
            var configuration = new Configuration(Datastores);

            var foo1 = new Foo { Name = "qux" };
            var foo = configuration.Load<Foo, Bar>(foo1, x => x.Name);
            foo.Succees.Verify().IsTrue();
            foo.Value.Verify().IsNotNull();
            foo.Value.Baz.Verify().IsEqual("bar");
        }      
    }
}
