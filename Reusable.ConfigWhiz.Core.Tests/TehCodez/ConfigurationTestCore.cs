using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.SmartConfig.Datastores;
using Reusable.SmartConfig.Tests.Common;
using Reusable.SmartConfig.Tests.Common.Configurations;

// ReSharper disable InconsistentNaming
// ReSharper disable BuiltInTypeReferenceStyle

namespace Reusable.SmartConfig.Tests
{
    [TestClass]
    public class ConfigurationTestCore : ConfigurationTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Datastores = new IDatastore[]
            {
                new Memory
                {
                    { "Bar.Qux", "quux" },
                    { "MyContainer.MySetting", "waldo" },
                    { "Qux", "corge" },
                    { "SimpleSetting", "foo" },
                },
                new Memory
                {
                    { "TestConsumer.Bar.Baz", "bar" },
                    { "TestConsumer[qux].Bar.Baz", "bar" }
                },
                new Memory().AddRange(SettingFactory.ReadSettings()),
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
            //var configuration = new Configuration(new[] { new TestDatastore("mock1", new List<Type>()) });
            //configuration.Get<TestConsumer, NonExistingConfiguration>();
        }

        [TestMethod]
        public void ctor_NoDefaultDatastore_Throws()
        {
            var ex = Assert.ThrowsException<AggregateException>(() =>
            {
                //var configuration = new Configuration(new[] { new Memory("mem1") });
                //configuration.Get<TestConsumer, TestContainer1>();
            });

            ex.InnerExceptions.First().Verify().IsInstanceOfType(typeof(SettingNotFoundException));
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

            //var renamed = configuration.Get<RenamedConfiguration>();
            //renamed.Bar.Verify().IsEqual("waldo");
        }

        #endregion

        [TestMethod]
        public void Load_SimpleSetting_Loaded()
        {
            var configuration = new Configuration(Datastores);

            //var result = configuration.Get<SimpleConfiguration>();
            //Assert.IsNotNull(result);
            //Assert.AreEqual("foo", result.SimpleSetting);
        }

        [TestMethod]
        public void Load_SameContainer_SameObject()
        {
            var configuration = new Configuration(Datastores);

            //var numeric1 = configuration.Get<EmptyConfiguration>();
            //var numeric2 = configuration.Get<EmptyConfiguration>();
            //Assert.IsNotNull(numeric1);
            //Assert.IsNotNull(numeric2);
            //Assert.AreSame(numeric1, numeric2);
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

        [TestMethod]
        public void Load_InstanceMembers_OnTheType_Loaded()
        {
            var config = new Configuration(new Memory
            {
                { "PublicProperty", "a" },
                { "PrivateProperty", "b" },
                { "PublicField", "c" },
                { "PrivateField", "d" },
                { "PrivateReadOnlyField", "e" },
            });

            var x = new InstanceClass(config);

            config.SetValue(() => x.PublicProperty);
            config.SetValue(() => x.PublicField);
            config.SetValue(() => x.PublicReadOnlyProperty);

            CollectionAssert.AreEqual(new[] { "a", null, "c", null, null, "f" }, x.GetValues().ToList());
        }

        [TestMethod]
        public void Load_InstanceMembers_InsideConstructor_Loaded()
        {
            var config = new Configuration(new Memory
            {
                { "PublicProperty", "a" },
                { "PrivateProperty", "b" },
                { "PublicField", "c" },
                { "PrivateField", "d" },
                { "PrivateReadOnlyField", "e" },
                { "PublicReadOnlyProperty", "f" },
            });

            var x = new InstanceClass(config);

            CollectionAssert.AreEqual(new[] { "a", "b", "c", "d", "e", "f" }, x.GetValues().ToList());
        }

        [TestMethod]
        public void Load_StaticMembers_Loaded()
        {
            var config = new Configuration(new Memory
            {
                { "PublicProperty", "a" },
                { "PrivateProperty", "b" },
                { "PublicField", "c" },
                { "PrivateField", "d" },
                { "PrivateReadOnlyField", "e" },
                { "PublicReadOnlyProperty", "f" },
            });

            config.SetValue(() => StaticClass.PublicProperty);
            config.SetValue(() => StaticClass.PublicField);
            config.SetValue(() => StaticClass.PublicReadOnlyProperty);

            CollectionAssert.AreEqual(new[] { "a", null, "c", null, null, "f" }, StaticClass.GetValues().ToList());
        }

        public class InstanceClass
        {
            public InstanceClass() { }

            public InstanceClass(IConfiguration config)
            {
                config.SetValue(() => PublicProperty);
                config.SetValue(() => PrivateProperty);
                config.SetValue(() => PublicField);
                config.SetValue(() => PrivateField);
                config.SetValue(() => PrivateReadOnlyField);
                config.SetValue(() => PublicReadOnlyProperty);
            }

            public string PublicProperty { get; set; }

            private string PrivateProperty { get; set; }

            public string PublicField;

            private string PrivateField;

            private readonly string PrivateReadOnlyField;

            public string PublicReadOnlyProperty { get; }

            public IEnumerable<object> GetValues()
            {
                yield return PublicProperty;
                yield return PrivateProperty;
                yield return PublicField;
                yield return PrivateField;
                yield return PrivateReadOnlyField;
                yield return PublicReadOnlyProperty;
            }
        }

        public static class StaticClass
        {
            public static string PublicProperty { get; set; }

            private static string PrivateProperty { get; set; }

            public static string PublicField;

            private static string PrivateField;

            private static readonly string PrivateReadOnlyField;

            public static string PublicReadOnlyProperty { get; }

            public static IEnumerable<object> GetValues()
            {
                yield return PublicProperty;
                yield return PrivateProperty;
                yield return PublicField;
                yield return PrivateField;
                yield return PrivateReadOnlyField;
                yield return PublicReadOnlyProperty;
            }
        }
    }
}
