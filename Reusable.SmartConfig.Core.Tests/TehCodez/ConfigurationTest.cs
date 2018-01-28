using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Exceptionize;
using Reusable.SmartConfig.Data;
using Reusable.Tester;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.SmartConfig.Tests
{
    [TestClass]
    public class ConfigurationTest
    {
        [TestMethod]
        public void ctor_EmptyDataStores_Throws()
        {
            var exception = Assert.That.ThrowsExceptionFiltered<DynamicException>(() => new Configuration(Enumerable.Empty<ISettingDataStore>()));

            Assert.AreEqual("IEnumerable<ISettingDataStore>ValidationException", exception.GetType().Name);
        }

        [TestMethod]
        public void GetValue_SettingExists_Value()
        {
            var dataStore = Mock.Create<ISettingDataStore>();

            var result = (dataStore, (ISetting)new Setting("foo") { Value = "bar" });

            var settingFinder = Mock.Create<ISettingFinder>();
            Mock
                .Arrange(() => settingFinder
                    .TryFindSetting(
                        Arg.IsAny<IEnumerable<ISettingDataStore>>(),
                        Arg.IsAny<SoftString>(),
                        Arg.IsAny<Type>(),
                        Arg.IsAny<SettingName>(),
                        out result
                    )
                )
                .Returns(true);

            var configuration = new Configuration(new[] { dataStore }, settingFinder);

            var actualValue = configuration.GetValue("foo", typeof(int), null);

            Assert.AreEqual("bar", actualValue);
        }

        [TestMethod]
        public void GetValue_SettingDoesNotExist_Throws()
        {
            var dataStore = Mock.Create<ISettingDataStore>();

            Mock
                .Arrange(() => dataStore.Read(Arg.IsAny<SoftString>(), Arg.IsAny<Type>()))
                .Returns(default(ISetting));

            var configuration = new Configuration(new[] { dataStore });

            var ex = Assert.That.ThrowsExceptionFiltered<DynamicException>(() => configuration.GetValue("foo", typeof(int), null));
            Assert.AreEqual("Setting 'foo' not found.", ex.Message);
        }

        [TestMethod]
        public void SetValue_SettingInitialized_WriteCalled()
        {
            var dataStore = Mock.Create<ISettingDataStore>();
            
            Mock
                .Arrange(() => dataStore.Write(Arg.IsAny<ISetting>()))
                .OccursOnce();

            var result = (dataStore, (ISetting)new Setting("baz.qux") { Value = 123 });

            var settingFinder = Mock.Create<ISettingFinder>();
            Mock
                .Arrange(() => settingFinder
                    .TryFindSetting(
                        Arg.IsAny<IEnumerable<ISettingDataStore>>(),
                        Arg.IsAny<SoftString>(),
                        Arg.IsAny<Type>(),
                        Arg.IsAny<SettingName>(),
                        out result
                    )
                )
                .Returns(true)
                .OccursOnce();

            var configuration = new Configuration(new[] { dataStore }, settingFinder);

            configuration.GetValue("foo.bar+baz.qux,quux", typeof(int), null);
            configuration.SetValue("foo.bar+baz.qux,quux", 123, null);

            settingFinder.Assert();
            dataStore.Assert();
        }

        [TestMethod]
        public void SetValue_SettingNotInitialized_WriteCalled()
        {
            var dataStore = Mock.Create<ISettingDataStore>();

            Mock
                .Arrange(() => dataStore.Read(Arg.IsAny<SoftString>(), Arg.IsAny<Type>()))
                .Returns(new Setting("baz.qux") { Value = 123 });

            Mock
                .Arrange(() => dataStore.Write(Arg.IsAny<ISetting>()))
                .OccursOnce();

            var result = (dataStore, (ISetting)new Setting("baz.qux") { Value = 123 });

            var settingFinder = Mock.Create<ISettingFinder>();
            Mock
                .Arrange(() => settingFinder
                    .TryFindSetting(
                        Arg.IsAny<IEnumerable<ISettingDataStore>>(),
                        Arg.IsAny<SoftString>(),
                        Arg.IsAny<Type>(),
                        Arg.IsAny<SettingName>(),
                        out result
                    )
                )
                .Returns(true)
                .OccursOnce();

            var configuration = new Configuration(new[] { dataStore }, settingFinder);

            //configuration.GetValue("foo.bar+baz.qux,quux", typeof(int), null);
            configuration.SetValue("foo.bar+baz.qux,quux", 123, null);

            settingFinder.Assert();
            dataStore.Assert();
        }
    }
}