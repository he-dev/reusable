using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Telerik.JustMock;
using Reusable.SmartConfig.Utilities;
using Telerik.JustMock.Helpers;

namespace Reusable.SmartConfig.Core.Utilities.Tests
{
    [TestClass]
    public class ConfigurationExtensionsTest
    {
        #region GetValue tests

        [TestMethod]
        public void GetValue_InstanceProperty_GotValue()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create("Reusable.SmartConfig.Core.Utilities.Tests+TestClass.Foo")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("foo")
                .OccursOnce();

            var testClass = new TestClass();
            var value = configuration.GetValue(() => testClass.Foo);

            configuration.Assert();
            Assert.AreEqual("foo", value);
        }

        [TestMethod]
        public void GetValue_InstancePropertyLocal_GotValue()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create("Reusable.SmartConfig.Core.Utilities.Tests+TestClass.Foo")),
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
                    Arg.Matches<SoftString>(name => name == SoftString.Create("Reusable.SmartConfig.Core.Utilities.Tests+TestClass.Bar")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("bar")
                .OccursOnce();

            var value = configuration.GetValue(() => TestClass.Bar);

            configuration.Assert();
            Assert.AreEqual("bar", value);
        }

        [TestMethod]
        public void GetValue_StaticPropertyLocal_GotValue()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create("Reusable.SmartConfig.Core.Utilities.Tests+TestClass.Bar")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("bar")
                .OccursOnce();

            TestClass.AssertGetValueStatic(configuration);
        }

        #endregion

        #region AssignValue(s) tests

        [TestMethod]
        public void AssignValue_InstanceProperty_Assigned()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create("Reusable.SmartConfig.Core.Utilities.Tests+TestClass.Foo")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("foo")
                .OccursOnce();

            var testClass = new TestClass();
            configuration.AssignValue(() => testClass.Foo);

            configuration.Assert();
            Assert.AreEqual("foo", testClass.Foo);
        }

        [TestMethod]
        public void AssignValue_InstancePropertyLocal_Assigned()
        {
            var configuration = Mock.Create<IConfiguration>();
            Mock
                .Arrange(() => configuration.GetValue(
                    Arg.Matches<SoftString>(name => name == SoftString.Create("Reusable.SmartConfig.Core.Utilities.Tests+TestClass.Foo")),
                    Arg.IsAny<Type>(),
                    Arg.IsAny<SoftString>())
                )
                .Returns("foo")
                .OccursOnce();

            var testClass = new TestClass();
            testClass.AssertAssignValue(configuration);
        }

        #endregion
    }

    internal class TestClass
    {
        public string Foo { get; set; }

        public static string Bar { get; set; }

        public void AssertGetValue(IConfiguration configuration)
        {
            var value = configuration.GetValue(() => Foo);

            configuration.Assert();
            Assert.AreEqual("foo", value);
        }

        public static void AssertGetValueStatic(IConfiguration configuration)
        {
            var value = configuration.GetValue(() => Bar);

            configuration.Assert();
            Assert.AreEqual("bar", value);
        }

        public void AssertAssignValue(IConfiguration configuration)
        {
            configuration.AssignValue(() => Foo);

            configuration.Assert();
            Assert.AreEqual("foo", Foo);
        }
    }
}
