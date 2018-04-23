using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Utilities.MSTest;

namespace Reusable.SmartConfig.Tests
{
    [TestClass]
    public class SettingNameByUsageGeneratorTest
    {
        [TestMethod]
        public void GenerateSettingNames_SettingNameWithoutInstance_TwoNames()
        {
            var names = new SettingNameByUsageGenerator().GenerateSettingNames("foo.bar+baz").ToList();
            
            Assert.That.Collection().CountEquals(2, names);
            Assert.AreEqual("baz", names.ElementAt(0));
            Assert.AreEqual("foo.bar+baz", names.ElementAt(1));
        }
        [TestMethod]
        public void GenerateSettingNames_SettingNameWithInstance_FourNames()
        {
            var names = new SettingNameByUsageGenerator().GenerateSettingNames("foo.bar+baz,qux").ToList();

            Assert.That.Collection().CountEquals(4, names);
            Assert.AreEqual("baz,qux", names.ElementAt(0));
            Assert.AreEqual("foo.bar+baz,qux", names.ElementAt(1));
            Assert.AreEqual("baz", names.ElementAt(2));
            Assert.AreEqual("foo.bar+baz", names.ElementAt(3));
        }
    }
}