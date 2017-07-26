using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.SmartConfig.Datastores;
using Reusable.SmartConfig.Tests.Common;

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

        #region Exceptions        

        [TestMethod]
        public void Select_SettingDoesNotExist_SettingNotFoundException()
        {
            Assert.ThrowsException<SettingNotFoundException>(() =>
            {
                var config = new Configuration();
                config.Select<int>(CaseInsensitiveString.Create("abc"));
            });
        }

        #endregion

        #region IO

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
                { "PublicReadOnlyProperty", "f" },
            });

            var x = new InstanceClass();

            config.Apply(() => x.PublicProperty);
            config.Apply(() => x.PublicField);
            config.Apply(() => x.PublicReadOnlyProperty);

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

            config.Apply(() => StaticClass.PublicProperty);
            config.Apply(() => StaticClass.PublicField);
            config.Apply(() => StaticClass.PublicReadOnlyProperty);

            CollectionAssert.AreEqual(new[] { "a", null, "c", null, null, "f" }, StaticClass.GetValues().ToList());
        }

        public class InstanceClass
        {
            public InstanceClass() { }

            public InstanceClass(IConfiguration config)
            {
                config.Apply(() => PublicProperty);
                config.Apply(() => PrivateProperty);
                config.Apply(() => PublicField);
                config.Apply(() => PrivateField);
                config.Apply(() => PrivateReadOnlyField);
                config.Apply(() => PublicReadOnlyProperty);
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
