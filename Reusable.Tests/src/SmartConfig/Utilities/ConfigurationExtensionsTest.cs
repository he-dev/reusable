using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Utilities;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.Tests.SmartConfig.Utilities
{
    [TestClass]
    public class ConfigurationExtensionsTest
    {
        private static readonly string Namespace = typeof(ConfigurationExtensionsTest).Namespace;

        #region GetValue tests

        [TestMethod]
        public void GetValue_InstanceProperty_GotValue()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create($"{Namespace}+TestClass.Foo")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("foo")
                .OccursOnce();

            var testClass = new TestClass();
            var value = configuration.GetValueFor(() => testClass.Foo);

            configuration.Assert();
            Assert.AreEqual("foo", value);
        }

        [TestMethod]
        public void GetValue_InstancePropertyLocal_GotValue()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create($"{Namespace}+TestClass.Foo")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("foo")
                .OccursOnce();

            var testClass = new TestClass();
            testClass.AssertGetValue(configuration);
        }

        [TestMethod]
        public void GetValue_StaticProperty_GotValue()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create($"{Namespace}+TestClass.Bar")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("bar")
                .OccursOnce();

            var value = configuration.GetValueFor(() => TestClass.Bar);

            configuration.Assert();
            Assert.AreEqual("bar", value);
        }

        [TestMethod]
        public void GetValue_StaticPropertyLocal_GotValue()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create($"{Namespace}+TestClass.Bar")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("bar")
                .OccursOnce();

            TestClass.AssertGetValueStatic(configuration);
        }

        [TestMethod]
        public void GetValue_SettingWithValidation_Valid()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.IsAny<SoftString>(),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("foo")
                .OccursOnce();

            var testClass3 = new TestClass3();
            var value = configuration.GetValueFor(() => testClass3.Foo);

            configuration.Assert();
            Assert.AreEqual("foo", value);
        }

        [TestMethod]
        public void GetValue_SettingWithValidation_Throws()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.IsAny<SoftString>(),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns((object)null)
                .OccursOnce();

            var testClass3 = new TestClass3();
            var validationException = Assert.ThrowsException<ValidationException>(() => configuration.GetValueFor(() => testClass3.Foo));
        }

        #endregion

        #region AssignValue(s) tests

        [TestMethod]
        public void AssignValue_InstanceProperty_Assigned()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create($"{Namespace}+TestClass.Foo")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("foo")
                .OccursOnce();

            var testClass = new TestClass();
            configuration.AssignValueTo(() => testClass.Foo);

            configuration.Assert();
            Assert.AreEqual("foo", testClass.Foo);
        }

        [TestMethod]
        public void AssignValue_InstancePropertyLocal_Assigned()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create($"{Namespace}+TestClass.Foo")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("foo")
                .OccursOnce();

            var testClass = new TestClass();
            testClass.AssertAssignValue(configuration);
        }

        [TestMethod]
        public void AssignValues_InstanceProperties_Assigned()
        {
            var counter = 1;

            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.IsAny<SoftString>(),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns(() => "Setting" + counter++)
                .Occurs(2);

            var testClass2 = new TestClass2();
            configuration.AssignValues(testClass2);

            configuration.Assert();
            Assert.AreEqual("Setting1", testClass2.Foo);
            Assert.AreEqual("Setting2", testClass2.Bar);

        }
        #endregion
    }

    internal class TestClass
    {
        public string Foo { get; set; }

        public static string Bar { get; set; }

        public void AssertGetValue(IConfiguration configuration)
        {
            var value = configuration.GetValueFor(() => Foo);

            configuration.Assert();
            Assert.AreEqual("foo", value);
        }

        public static void AssertGetValueStatic(IConfiguration configuration)
        {
            var value = configuration.GetValueFor(() => Bar);

            configuration.Assert();
            Assert.AreEqual("bar", value);
        }

        public void AssertAssignValue(IConfiguration configuration)
        {
            configuration.AssignValueTo(() => Foo);

            configuration.Assert();
            Assert.AreEqual("foo", Foo);
        }
    }

    internal class TestClass2
    {
        [SmartSetting]
        public string Foo { get; set; }

        [SmartSetting]
        public string Bar { get; set; }
    }

    internal class TestClass3
    {
        [Required]
        public string Foo { get; set; }
    }
}
