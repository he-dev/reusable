using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Data;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.SmartConfig.Tests
{
    [TestClass]
    public class SettingDataStoreTest
    {
        [TestMethod]
        public void Read_Any_ReadCoreCalled()
        {
            var settingConverter = Mock.Create<ISettingConverter>();

            var dataStore = Mock.Create<SettingDataStore>(Behavior.CallOriginal, settingConverter);
            Mock
                .NonPublic
                .Arrange<ISetting>(dataStore, "ReadCore", ArgExpr.IsAny<IEnumerable<SoftString>>())
                .MustBeCalled();

            dataStore.Read("foo", typeof(string));

            dataStore.Assert();
        }

        [TestMethod]
        public void Read_Any_SettingConverterDeserializeCalled()
        {
            var settingConverter = Mock.Create<ISettingConverter>();
            Mock
                .Arrange(() => settingConverter.Deserialize(Arg.IsAny<object>(), Arg.IsAny<Type>()))
                .OccursOnce();

            var dataStore = Mock.Create<SettingDataStore>(Behavior.CallOriginal, settingConverter);
            Mock
                .NonPublic
                .Arrange<ISetting>(dataStore, "ReadCore", ArgExpr.IsAny<IEnumerable<SoftString>>())
                .Returns(new Setting("foo", "bar"));

            dataStore.Read("foo", typeof(string));

            settingConverter.Assert();
        }

        [TestMethod]
        public void Write_Any_WriteCoreCalled()
        {
            var setting = new Setting("foo", "bar");
            var settingConverter = Mock.Create<ISettingConverter>();
            var dataStore = Mock.Create<SettingDataStore>(Behavior.CallOriginal, settingConverter);
            Mock
                .NonPublic
                .Arrange(dataStore, "WriteCore", ArgExpr.IsAny<ISetting>())
                .OccursOnce();

            dataStore.Write(setting);

            dataStore.Assert();
        }

        [TestMethod]
        public void Write_Any_SettingConverterSerializeCalled()
        {
            var setting = new Setting("foo", "bar");

            var settingConverter = Mock.Create<ISettingConverter>();
            Mock
                .Arrange(() => settingConverter.Serialize(Arg.IsAny<object>()))
                .OccursOnce();

            var dataStore = Mock.Create<SettingDataStore>(Behavior.CallOriginal, settingConverter);
            Mock
                .NonPublic
                .Arrange(dataStore, "WriteCore", ArgExpr.IsAny<ISetting>());

            dataStore.Write(setting);

            settingConverter.Assert();
        }
    }
}
