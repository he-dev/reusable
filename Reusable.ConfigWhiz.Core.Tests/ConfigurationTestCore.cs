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

        [TestMethod]
        public void Load_Numeric_Success()
        {
            var configuration = new Configuration(Datastores);

            var load = configuration.Load<Foo, Numeric>();
            load.Succees.Verify().IsTrue();
            var foo = load.Value;
            foo.Verify().IsNotNull();
            foo.SByte.Verify().IsEqual(SByte.MaxValue);
            foo.Byte.Verify().IsEqual(Byte.MaxValue);
            foo.Char.Verify().IsEqual(Char.MaxValue);
            foo.Int16.Verify().IsEqual(Int16.MaxValue);
            foo.Int32.Verify().IsEqual(Int32.MaxValue);
            foo.Int64.Verify().IsEqual(Int64.MaxValue);
            foo.UInt16.Verify().IsEqual(UInt16.MaxValue);
            foo.UInt32.Verify().IsEqual(UInt32.MaxValue);
            foo.UInt64.Verify().IsEqual(UInt64.MaxValue);
            foo.Single.Verify().IsEqual(Single.MaxValue);
            foo.Double.Verify().IsEqual(Double.MaxValue);
            foo.Decimal.Verify().IsEqual(Decimal.MaxValue);
        }
    }
}
