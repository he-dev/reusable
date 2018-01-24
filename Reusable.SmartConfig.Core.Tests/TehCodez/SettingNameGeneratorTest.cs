using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Tests.Services
{
    [TestClass]
    public class SettingNameGeneratorTest
    {
        [TestMethod]
        public void GenerateNames_BaseName_NamesByFrequency()
        {
            var settingNameGenerator = new SettingNameByUsageGenerator();

            CollectionAssert.AreEqual(
                new[]
                {
                    "baz.qux",
                    "qux",
                    "foo.bar+baz.qux",
                },
                settingNameGenerator.GenerateSettingNames(SettingName.Parse("foo.bar+baz.qux")).Select(name => (string)name).ToList());
        }

        [TestMethod]
        public void GenerateNames_BaseNameWithInstance_NamesByFrequency()
        {
            var settingNameGenerator = new SettingNameByUsageGenerator();

            CollectionAssert.AreEqual(
                new[]
                {
                    "baz.qux,quux",
                    "qux,quux",
                    "foo.bar+baz.qux,quux",
                    "baz.qux",
                    "qux",
                    "foo.bar+baz.qux",
                },
                settingNameGenerator.GenerateSettingNames(SettingName.Parse("foo.bar+baz.qux,quux")).Select(name => (string)name).ToList());
        }
    }
}
