using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.SmartConfig.DataStores.Tests
{
    [TestClass]
    public class AppSettingsTest
    {
        [TestInitialize]
        public void TestInitialize()
        {
            Seed();
        }

        [TestMethod]
        public void Read_ByName_Setting()
        {
            var converter = Mock.Create<ISettingConverter>();
            Mock
                .Arrange(() => converter.Deserialize(Arg.IsAny<object>(), Arg.IsAny<Type>()))
                .Returns(obj => obj)
                .Occurs(2);

            var appSettings = new AppSettings(converter);

            var foo = appSettings.Read("foo", typeof(string));
            var baz = appSettings.Read("baz", typeof(string));

            converter.Assert();
            Assert.AreEqual("bar", foo.Value);
            Assert.AreEqual("qux", baz.Value);
        }

        private static void Seed()
        {
            var data = new(string Key, string Value)[]
            {
                ("foo", "bar"),
                ("baz", "qux")
            };

            var exeConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            exeConfiguration.AppSettings.Settings.Clear();
            exeConfiguration.ConnectionStrings.ConnectionStrings.Clear();

            foreach (var (key, value) in data)
            {
                exeConfiguration.AppSettings.Settings.Add(key, value);
            }

            exeConfiguration.Save(ConfigurationSaveMode.Minimal);
        }
    }
}
