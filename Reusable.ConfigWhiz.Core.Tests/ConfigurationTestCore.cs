using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Core.Tests.Data;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Data.Annotations;
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

        #region Exception tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ctor_NoDatastores_Throws()
        {
            var configuration = new Configuration(Enumerable.Empty<IDatastore>());
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicateDatatastoreException))]
        public void ctor_DuplicateDatastores_Throws()
        {
            var configuration = new Configuration(new Memory("mem1"), new Memory("mem1"));
        }

        [TestMethod]
        [ExpectedException(typeof(DatastoreReadException))]
        public void ctor_CannotReadFromDatastore_Throws()
        {
            var configuration = new Configuration(new MockDatastore("mock1", new List<Type>()));
            configuration.Load<Foo, Bar>();
        }

        [TestMethod]
        public void ctor_NoDefaultDatastore_Throws()
        {
            var ex = new Action(() =>
            {
                var configuration = new Configuration(new Memory("mem1"));
                configuration.Load<Foo, Baz>();
            }).Verify().Throws<AggregateException>();

            ex.InnerExceptions.First().Verify().IsInstanceOfType(typeof(DatastoreNotFoundException));
        }

        [TestMethod]
        public void ctor_ItemizedSettingWithInvalidType_Throws()
        {
            var ex = new Action(() =>
            {
                var configuration = new Configuration(new Memory("mem1")
                {
                    { $@"{typeof(Foo).Namespace}.Foo.Baz2.Qux2", "quux" }
                });
                configuration.Load<Foo, Baz2>();
            }).Verify().Throws<AggregateException>();

            ex.InnerExceptions.First().Verify().IsInstanceOfType(typeof(UnsupportedItemizedTypeException));
        }

        #endregion

        #region Load tests

        [TestMethod]
        public void Load_ConsumerWithoutName_Success()
        {
            var configuration = new Configuration(Datastores);

            var bar = configuration.Load<Foo, Bar>();
            bar.Verify().IsNotNull();
            bar.Baz.Verify().IsEqual("bar");
        }

        [TestMethod]
        public void Load_ConsumerWithName_Success()
        {
            var configuration = new Configuration(Datastores);

            var foo1 = new Foo { Name = "qux" };
            var bar = configuration.Load<Foo, Bar>(foo1, x => x.Name);
            bar.Verify().IsNotNull();
            bar.Baz.Verify().IsEqual("bar");
        }

        #endregion

  

        private class MockDatastore : Datastore
        {
            public MockDatastore(string name, IEnumerable<Type> supportedTypes) : base(name, supportedTypes)
            {
            }

            protected override ICollection<ISetting> ReadCore(SettingPath settingPath)
            {
                throw new NotImplementedException();
            }

            protected override int WriteCore(IGrouping<SettingPath, ISetting> settings)
            {
                throw new NotImplementedException();
            }
        }

        [DefaultDatastore("Datastore1")]
        private class Baz
        {
            public string Qux { get; set; }
        }

        [DefaultDatastore("Datastore1")]
        private class Baz2
        {
            [Itemized]
            public string Qux2 { get; set; }
        }
    }
}
