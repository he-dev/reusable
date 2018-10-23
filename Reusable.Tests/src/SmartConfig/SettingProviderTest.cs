using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Data;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.Tests.SmartConfig
{
    [TestClass]
    public class SettingProviderTest
    {
        [TestMethod]
        public void Read_CallsServicesAndInternalOverload()
        {
            var settingName = new SettingName("prefix", "namespace", "type", "member", "instance");
            var settingNameFactory = Mock.Create<ISettingNameFactory>();
            var settingConverter = Mock.Create<ISettingConverter>();

            settingNameFactory
                .Arrange(x => x.CreateProviderSettingName(
                    Arg.Matches<SettingName>(sn => sn == settingName),
                    Arg.IsAny<SettingProviderNaming>())
                )
                .Returns(settingName)
                .OccursOnce();

            settingConverter
                .Arrange(x => x.Deserialize(
                    Arg.Matches<object>(value => value == (object)"TestValue"),
                    Arg.Matches<Type>(type => type == typeof(string))
                ))
                .Returns("TestValue")
                .OccursOnce();

            var settingProvider = Mock.Create<SettingProvider>(Behavior.CallOriginal, settingNameFactory, settingConverter);

            Mock
                .NonPublic
                .Arrange<ISetting>(
                    settingProvider,
                    nameof(SettingProvider.Read),
                    ArgExpr.Matches<SettingName>(sn => sn == settingName)
                )
                .Returns(new Setting(settingName.ToString(), "TestValue"))
                .MustBeCalled();

            var setting = settingProvider.Read(new SelectQuery(settingName, typeof(string)));

            settingNameFactory.Assert();
            settingProvider.Assert();
            settingConverter.Assert();

            Assert.IsNotNull(setting);
            Assert.AreEqual(settingName, setting.Name);
            Assert.AreEqual("TestValue", setting.Value);
        }


        [TestMethod]
        public void Write_CallsServicesAndInternalOverload()
        {
            var settingName = new SettingName("prefix", "namespace", "type", "member", "instance");
            var settingNameFactory = Mock.Create<ISettingNameFactory>();
            var settingConverter = Mock.Create<ISettingConverter>();

            settingNameFactory
                .Arrange(x => x.CreateProviderSettingName(
                    Arg.Matches<SettingName>(sn => sn == settingName),
                    Arg.IsAny<SettingProviderNaming>())
                )
                .Returns(settingName)
                .OccursOnce();

            settingConverter
                .Arrange(x => x.Serialize(
                    Arg.Matches<object>(value => value == (object)"TestValue"))
                )
                .Returns("TestValue")
                .OccursOnce();

            var settingProvider = Mock.Create<SettingProvider>(Behavior.CallOriginal, settingNameFactory, settingConverter);

            Mock
                .NonPublic
                .Arrange(
                    settingProvider,
                    nameof(SettingProvider.Write),
                    ArgExpr.Matches<ISetting>(s => s.Name == settingName)
                )
                .MustBeCalled();

            settingProvider.Write(new UpdateQuery(settingName, "TestValue"));

            settingNameFactory.Assert();
            settingProvider.Assert();
            settingConverter.Assert();            
        }
    }
}