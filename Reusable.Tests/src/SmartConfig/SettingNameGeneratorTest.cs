using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Data;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.SmartConfig
{
    [TestClass]
    public class SettingNameGeneratorTest
    {
        [TestMethod]
        public void GenerateNames_BaseName_NamesByFrequency()
        {
            var settingNameGenerator = new SettingNameByUsageGenerator();

            var expectedNames = new[]
            {
                "baz.qux",
                "qux",
                "foo.bar+baz.qux",
            };
            var actualNames = settingNameGenerator.GenerateSettingNames(SettingName.Parse("foo.bar+baz.qux")).Select(name => (string)name);

            Assert.That.Collection().AreEqual(expectedNames, actualNames);
        }

        [TestMethod]
        public void GenerateNames_BaseNameWithInstance_NamesByFrequency()
        {
            var settingNameGenerator = new SettingNameByUsageGenerator();

            var expectedNames = new []
            {
                "baz.qux,quux",
                "qux,quux",
                "foo.bar+baz.qux,quux",
                "baz.qux",
                "qux",
                "foo.bar+baz.qux",
            };

            var actualNames = settingNameGenerator.GenerateSettingNames(SettingName.Parse("foo.bar+baz.qux,quux")).Select(name => (string)name);

            Assert.That.Collection().AreEqual(expectedNames, actualNames);
        }
    }
}
