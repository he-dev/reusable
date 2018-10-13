using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Data;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.Tests.SmartConfig
{
    [TestClass]
    public class FirstSettingFinderTest
    {
        private static readonly ISettingFinder FirstSettingFinder = new FirstSettingFinder();
        
        [TestMethod]
        public void CanFindSettingByName()
        {
            var provider1 = Mock.Create<ISettingProvider>();
            var provider2 = Mock.Create<ISettingProvider>();
            var provider3 = Mock.Create<ISettingProvider>();

            provider1
                .Arrange(x => x.Read(Arg.IsAny<SelectQuery>()))
                .Returns(default(ISetting));

            provider2
                .Arrange(x => x.Read(Arg.Matches<SelectQuery>(arg => arg.SettingName == SettingName.Parse("Type.Member"))))
                .Returns(Setting.Create("Type.Member", "abc"));

            provider3
                .Arrange(x => x.Read(Arg.IsAny<SelectQuery>()))
                .Returns(default(ISetting));

            var settingFound = FirstSettingFinder.TryFindSetting
            (
                new SelectQuery(SettingName.Parse("Type.Member"), typeof(string)),
                new[] { provider1, provider2, provider3 },
                out var result
            );

            Assert.IsTrue(settingFound);
            Assert.AreSame(provider2, result.SettingProvider);
            Assert.AreEqual("abc", result.Setting.Value);
        }

        [TestMethod]
        public void DoesNotFindNotExistingSetting()
        {
            var provider = Mock.Create<ISettingProvider>();
            provider
                .Arrange(x => x.Read(Arg.IsAny<SelectQuery>()))
                .Returns(default(ISetting));

            var settingFound = FirstSettingFinder.TryFindSetting
            (
                new SelectQuery(SettingName.Parse("Type.Member"), typeof(string)),
                new[] { provider },
                out var result
            );

            Assert.IsFalse(settingFound);
        }
    }
}