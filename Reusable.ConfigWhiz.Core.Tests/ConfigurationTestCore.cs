using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Datastores;
using Reusable.ConfigWhiz.Tests.Common;
using CData = Reusable.ConfigWhiz.Tests.Common.Data;
using Reusable.Extensions;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.ConfigWhiz.Services;
using Reusable.ConfigWhiz.Tests.Common.Configurations;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public class ConfigurationTestCore : ConfigurationTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Datastores = new IDatastore[]
            {
                new Memory("Memory1")
                {
                    { "Bar.Qux", "quux" },
                    { "MyContainer.MySetting", "waldo" },
                    { "Qux", "corge" }
                },
                new Memory("Memory2")
                {
                    { "TestConsumer.Bar.Baz", "bar" },
                    { "TestConsumer[qux].Bar.Baz", "bar" }
                },
                new Memory("Memory3").AddRange(SettingFactory.ReadSettings()),
            };
        }

        #region Exception tests

        [TestMethod]
        //[ExpectedException(typeof(ArgumentException))]
        public void ctor_NoDatastores_Throws()
        {
            var configuration = new Configuration(Enumerable.Empty<IDatastore>());
        }

        //[TestMethod]
        //[ExpectedException(typeof(DuplicateDatatastoreException))]
        //public void ctor_DuplicateDatastores_Throws()
        //{
        //    var configuration = new Configuration(new[] { new Memory("mem1"), new Memory("mem1") });
        //}

        [TestMethod]
        [ExpectedException(typeof(DatastoreReadException))]
        public void ctor_CannotReadFromDatastore_Throws()
        {
            var configuration = new Configuration(new[] { new TestDatastore("mock1", new List<Type>()) });
            configuration.Get<TestConsumer, NonExistingConfiguration>();
        }

        [TestMethod]
        public void ctor_NoDefaultDatastore_Throws()
        {
            var ex = Assert.ThrowsException<AggregateException>(() =>
            {
                var configuration = new Configuration(new[] { new Memory("mem1") });
                configuration.Get<TestConsumer, CData.TestContainer1>();
            });

            ex.InnerExceptions.First().Verify().IsInstanceOfType(typeof(DatastoreNotFoundException));
        }

        //[TestMethod]
        //public void ctor_ItemizedSettingWithInvalidType_Throws()
        //{
        //    var ex = new Action(() =>
        //    {
        //        var configuration = new Configuration(new[] { new Memory("mem1")
        //        {
        //            { $"{typeof(TestConsumer).Namespace}.TestConsumer.TestContainer2.TestSetting2", "quux" }
        //        }});
        //        configuration.Get<TestConsumer, TestContainer2>();
        //    }).Verify().Throws<AggregateException>();

        //    ex.InnerExceptions.First().Verify().IsInstanceOfType(typeof(UnsupportedItemizedTypeException));
        //}

        #endregion

        #region Other

        [TestMethod]
        public void Load_Renamed_Success()
        {
            var configuration = new Configuration(Datastores);

            var renamed = configuration.Get<RenamedConfiguration>();
            renamed.Bar.Verify().IsEqual("waldo");
        }

        #endregion

        [TestMethod]
        public void Load_SameContainer_SameObject()
        {
            var configuration = new Configuration(Datastores);

            var numeric1 = configuration.Get<EmptyConfiguration>();
            var numeric2 = configuration.Get<EmptyConfiguration>();
            Assert.IsNotNull(numeric1);
            Assert.IsNotNull(numeric2);
            Assert.AreSame(numeric1, numeric2);
        }

        //[TestMethod]
        //public void Load_DataSource_Provider_SameObject()
        //{
        //    var configuration = new Configuration(Datastores);

        //    var numeric1 = configuration.Get<TestConsumer, NumericConfiguration>();
        //    var numeric2 = configuration.Get<TestConsumer, NumericConfiguration>(); //DataOrigin.Provider);
        //    Assert.IsNotNull(numeric1);
        //    Assert.IsNotNull(numeric2);
        //    Assert.AreSame(numeric1, numeric2);
        //}
    }
}
