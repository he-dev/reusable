using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Core.Tests.Data;
using Reusable.ConfigWhiz.Datastores;
using Reusable.Extensions;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public class ConfigurationTestCore : ConfigurationTestDatastore
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var ns = typeof(Foo).Namespace;

            Datastores = new IDatastore[]
            {
                new Memory("Memory1")
                {
                    { $@"{ns}.Foo.Bar.Qux", "quux" }
                },
                new Memory("Memory2")
                {
                    { $@"{ns}.Foo.Bar.Baz", "bar" },
                    { $@"{ns}.Foo[""qux""].Bar.Baz", "bar" }
                },
                new Memory("Memory3").AddRange(SettingFactory.ReadSettings<Foo>()), 
            };
        }

        [TestMethod]
        public void Load_ConsumerWithoutName_Success()
        {
            var configuration = new Configuration(Datastores);

            var load = configuration.Load<Foo, Bar>();
            load.Succees.Verify().IsTrue();
            load.Value.Verify().IsNotNull();
            load.Value.Baz.Verify().IsEqual("bar");
        }

        [TestMethod]
        public void Load_ConsumerWithName_Success()
        {
            var configuration = new Configuration(Datastores);

            var foo1 = new Foo { Name = "qux" };
            var load = configuration.Load<Foo, Bar>(foo1, x => x.Name);
            load.Succees.Verify().IsTrue();
            load.Value.Verify().IsNotNull();
            load.Value.Baz.Verify().IsEqual("bar");
        }

        
    }
}
