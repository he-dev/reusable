using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Exceptionize;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Binding;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Tests.Common;
using Reusable.Tester;

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
                new InMemory
                {
                    { "TestSetting1", "abc" },
                    { "TestSetting3", "xyz" },
                    { "TestSetting4", null },
                    { "TestSetting5", null },
                    { "DerivedClass.BaseSetting", "jkl" },                    
                }, 
                new InMemory
                {
                    { "TestConsumer.Bar.Baz", "bar" },
                    { "TestConsumer[qux].Bar.Baz", "bar" }
                },
                new InMemory().AddRange(TestSettingRepository.Settings),
            };
        }

        #region Exceptions        

        [TestMethod]
        public void Select_SettingDoesNotExist_SettingNotFoundException()
        {
            Assert.That.ThrowsExceptionFiltered<DynamicException>(() =>
            {
                var config = new Configuration(options =>
                {
                    options
                        .UseJsonConverter()
                        .UseInMemory(Enumerable.Empty<ISetting>());
                });
                config.Select<int>(SoftString.Create("abc"));
                Assert.Fail();
            },
            ex => ex.NameEquals("SettingNotFoundException"));
        }

        #endregion

        #region IO

        [TestMethod]
        public void Select_ByVariable_Value()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });

            var testClass = new TestClass();
            var value = config.Select(() => testClass.TestSetting1);
            Assert.AreEqual("abc", value);
        }

        [TestMethod]
        public void Select_ByType_Value()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });
            var value = config.From<TestClass>().Select(x => x.TestSetting1);
            Assert.AreEqual("abc", value);
        }

        [TestMethod]
        public void Select_Renamed_Value()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });
            var value = config.From<TestClass>().Select(x => x.TestSetting2);
            Assert.AreEqual("xyz", value);
        }

        [TestMethod]
        public void Select_Default_Value()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });
            var value = config.From<TestClass>().Select(x => x.TestSetting4);
            Assert.AreEqual("jkl", value);
        }

        [TestMethod]
        public void Select_Required_Validated()
        {
            Assert.ThrowsException<ValidationException>(() =>
            {
                var config = new Configuration(options =>
                {
                    options
                        .UseJsonConverter()
                        .UseMultiple(Datastores);
                });
                var value = config.From<TestClass>().Select(x => x.TestSetting5);
            });
        }

        [TestMethod]
        public void Bind_Properties_Value()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });
            var t = new TestClass1();
            var value = config.Bind(t);
            Assert.AreEqual("abc", t.TestSetting1);
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

        private class TestClass1
        {
            [SmartSetting]
            public string TestSetting1 { get; set; }
        }

        #endregion       

        [TestMethod]
        public void Load_InstanceMembers_OnTheType_Loaded()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseInMemory(new[]
                    {
                        new Setting { Name = "PublicProperty", Value = "a" },
                        new Setting { Name = "PrivateProperty", Value = "b" },
                        new Setting { Name = "PublicField", Value = "c" },
                        new Setting { Name = "PrivateField", Value = "d" },
                        new Setting { Name = "PrivateReadOnlyField", Value = "e" },
                        new Setting { Name = "PublicReadOnlyProperty", Value = "f" },
                    });
            });            

            var x = new InstanceClass();

            config.Bind(() => x.PublicProperty);
            config.Bind(() => x.PublicField);
            config.Bind(() => x.PublicReadOnlyProperty);

            CollectionAssert.AreEqual(new[] { "a", null, "c", null, null, "f" }, x.GetValues().ToList());
        }

        [TestMethod]
        public void Load_InstanceMembers_InsideConstructor_Loaded()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseInMemory(new[]
                    {
                        new Setting { Name = "PublicProperty", Value = "a" },
                        new Setting { Name = "PrivateProperty", Value = "b" },
                        new Setting { Name = "PublicField", Value = "c" },
                        new Setting { Name = "PrivateField", Value = "d" },
                        new Setting { Name = "PrivateReadOnlyField", Value = "e" },
                        new Setting { Name = "PublicReadOnlyProperty", Value = "f" },
                    });
            });

            var x = new InstanceClass(config);

            CollectionAssert.AreEqual(new[] { "a", "b", "c", "d", "e", "f" }, x.GetValues().ToList());
        }

        [TestMethod]
        public void Load_StaticMembers_Loaded()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseInMemory(new[]
                    {
                        new Setting { Name = "PublicProperty", Value = "a" },
                        new Setting { Name = "PrivateProperty", Value = "b" },
                        new Setting { Name = "PublicField", Value = "c" },
                        new Setting { Name = "PrivateField", Value = "d" },
                        new Setting { Name = "PrivateReadOnlyField", Value = "e" },
                        new Setting { Name = "PublicReadOnlyProperty", Value = "f" },
                    });
            });

            config.Bind(() => StaticClass.PublicProperty);
            config.Bind(() => StaticClass.PublicField);
            config.Bind(() => StaticClass.PublicReadOnlyProperty);

            CollectionAssert.AreEqual(new[] { "a", null, "c", null, null, "f" }, StaticClass.GetValues().ToList());
        }

        [TestMethod]
        public void Select_BaseSettingFromDerivedClass_Selected()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });
            var derived = new DerivedClass(config);
            config.Bind(() => derived.BaseSetting);
            Assert.AreEqual("jkl", derived.BaseSetting);
        }

        public class InstanceClass
        {
            public InstanceClass() { }

            public InstanceClass(IConfiguration config)
            {
                //PublicProperty = config.Select(() => PublicProperty);

                config.Bind(() => PublicProperty);
                config.Bind(() => PrivateProperty);
                config.Bind(() => PublicField);
                config.Bind(() => PrivateField);
                config.Bind(() => PrivateReadOnlyField);
                config.Bind(() => PublicReadOnlyProperty);
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

        public class BaseClass
        {
            public string BaseSetting { get; set; }
        }

        public class DerivedClass : BaseClass
        {
            public DerivedClass(IConfiguration config)
            {
                config.Bind(() => BaseSetting);
            }
        }
    }
}