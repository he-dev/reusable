using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Annotations;
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
                    { "TestSetting1", "abc" },
                    { "TestSetting3", "xyz" },
                    { "TestSetting4", null },
                    { "TestSetting5", null },
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
        public void Select_ByVariable_Value()
        {
            var config = new Configuration(Datastores);
            var testClass = new TestClass();
            var value = config.Select(() => testClass.TestSetting1);
            Assert.AreEqual("abc", value);
        }

        [TestMethod]
        public void Select_ByType_Value()
        {
            var config = new Configuration(Datastores);
            var value = config.For<TestClass>().Select(x => x.TestSetting1);
            Assert.AreEqual("abc", value);
        }

        [TestMethod]
        public void Select_Renamed_Value()
        {
            var config = new Configuration(Datastores);
            var value = config.For<TestClass>().Select(x => x.TestSetting2);
            Assert.AreEqual("xyz", value);
        }        

        [TestMethod]
        public void Select_Default_Value()
        {
            var config = new Configuration(Datastores);
            var value = config.For<TestClass>().Select(x => x.TestSetting4);
            Assert.AreEqual("jkl", value);
        }

        [TestMethod]
        public void Select_Required_Validated()
        {
            Assert.ThrowsException<ValidationException>(() =>
            {
                var config = new Configuration(Datastores);
                var value = config.For<TestClass>().Select(x => x.TestSetting5);
            });
        }

        private class TestClass
        {
            public string TestSetting1 { get; set; }

            [SmartSetting(Name = "TestSetting3")]
            public string TestSetting2 { get; set; }

            [DefaultValue("jkl")]
            public string TestSetting4 { get; set; }

            [Required]
            public string TestSetting5 { get; set; }
        }

        #endregion       

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
                PublicProperty = config.Select(() => PublicProperty);

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
