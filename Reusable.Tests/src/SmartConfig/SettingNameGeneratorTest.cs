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
        private static readonly ISettingNameGenerator Generator = new SettingNameGenerator();

        [TestMethod]
        public void GenerateSettingNames_CanGenerateNamesWithoutInstance()
        {
            var expectedNames = new[]
            {
                "qux",
                "baz.qux",
                "foo.bar+baz.qux",
            };
            var actualNames =
                Generator
                    .GenerateSettingNames(SettingName.Parse("foo.bar+baz.qux"))
                    .Select(name => name.ToString())
                    .ToList();

            Assert.That.Collection().AreEqual(expectedNames, actualNames);
        }

        [TestMethod]
        public void GenerateSettingNames_CanGenerateNamesWithInstance()
        {
            var expectedNames = new[]
            {
                "qux,waldo",
                "baz.qux,waldo",
                "foo.bar+baz.qux,waldo",
            };

            var actualNames =
                Generator
                    .GenerateSettingNames(SettingName.Parse("foo.bar+baz.qux,waldo"))
                    .Select(name => name.ToString())
                    .ToList();

            Assert.That.Collection().AreEqual(expectedNames, actualNames);
        }
    }
}