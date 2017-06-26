using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Datastores;
using Reusable.ConfigWhiz.IO;
using Reusable.ConfigWhiz.Tests.Common;
using CData = Reusable.ConfigWhiz.Tests.Common.Data;
using Reusable.Extensions;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var ns = typeof(TestConsumer).Namespace;

            Datastores = new IDatastore[]
            {
                new Memory("Memory1")
                {
                    { $"{ns}.TestConsumer.Bar.Qux", "quux" },
                    { $"{ns}.TestConsumer.MyContainer.MySetting", "waldo" },
                    { $"{ns}.TestConsumer.Qux", "corge" }
                },
                new Memory("Memory2")
                {
                    { $"{ns}.TestConsumer.Bar.Baz", "bar" },
                    { $"{ns}.TestConsumer[\"qux\"].Bar.Baz", "bar" }
                },
                new Memory("Memory3").AddRange(SettingFactory.ReadSettings()),
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
            var configuration = new Configuration(new TestDatastore("mock1", new List<Type>()));
            configuration.Resolve<TestConsumer, CData.Bar>();
        }

        [TestMethod]
        public void ctor_NoDefaultDatastore_Throws()
        {
            var ex = new Action(() =>
            {
                var configuration = new Configuration(new Memory("mem1"));
                configuration.Resolve<TestConsumer, CData.TestContainer1>();
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
                    { $"{typeof(TestConsumer).Namespace}.TestConsumer.TestContainer2.TestSetting2", "quux" }
                });
                configuration.Resolve<TestConsumer, CData.TestContainer2>();
            }).Verify().Throws<AggregateException>();

            ex.InnerExceptions.First().Verify().IsInstanceOfType(typeof(UnsupportedItemizedTypeException));
        }

        #endregion

        #region Load tests

        [TestMethod]
        public void Load_ConsumerWithoutName_Success()
        {
            var configuration = new Configuration(Datastores);

            var bar = configuration.Resolve<TestConsumer, CData.Bar>();
            bar.Verify().IsNotNull();
            bar.Baz.Verify().IsEqual("bar");
        }

        [TestMethod]
        public void Load_ConsumerWithName_Success()
        {
            var configuration = new Configuration(Datastores);

            var consumer = new TestConsumer { Name = "qux" };
            var bar = configuration.Resolve<TestConsumer, CData.Bar>(consumer, x => x.Name);
            bar.Verify().IsNotNull();
            bar.Baz.Verify().IsEqual("bar");
        }

        [TestMethod]
        public void Load_SameConsumerAndContaierName_DoubleNameSkipped()
        {
            var configuration = new Configuration(Datastores);

            var container = configuration.Resolve<TestConsumer, CData.TestConsumer>();
            container.Verify().IsNotNull();
            container.Qux.Verify().IsEqual("corge");
        }

        #endregion

        #region Other

        [TestMethod]
        public void Load_Renamed_Success()
        {
            var configuration = new Configuration(Datastores);

            var renamed = configuration.Resolve<TestConsumer, CData.Renamed>();
            renamed.Bar.Verify().IsEqual("waldo");
        }

        #endregion

    }
}
