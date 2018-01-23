using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Tests.Data
{
    [TestClass]
    public class SettingNameTest
    {
        // foo.bar+baz.qux,quux

        [TestMethod]
        public void Parse_Property_Name()
        {
            var settingName = SettingName.Parse("qux");

            Assert.IsNull(settingName.Namespace);
            Assert.IsNull(settingName.Type);
            Assert.AreEqual("qux", settingName.Property);
            Assert.IsNull(settingName.Instance);
            Assert.AreEqual("qux", settingName.ToString());
        }

        [TestMethod]
        public void Parse_TypeProperty_Name()
        {
            var settingName = SettingName.Parse("baz.qux");

            Assert.IsNull(settingName.Namespace);
            Assert.AreEqual("baz", settingName.Type);
            Assert.AreEqual("qux", settingName.Property);
            Assert.IsNull(settingName.Instance);
            Assert.AreEqual("baz.qux", settingName.ToString());
        }

        [TestMethod]
        public void Parse_NamespaceTypeProperty_Name()
        {
            var settingName = SettingName.Parse("foo.bar+baz.qux");

            Assert.AreEqual("foo.bar", settingName.Namespace);
            Assert.AreEqual("baz", settingName.Type);
            Assert.AreEqual("qux", settingName.Property);
            Assert.IsNull(settingName.Instance);
            Assert.AreEqual("foo.bar+baz.qux", settingName.ToString());
        }

        [TestMethod]
        public void Parse_NamespaceTypePropertyInstance_Name()
        {
            var settingName = SettingName.Parse("foo.bar+baz.qux,quux");

            Assert.AreEqual("foo.bar", settingName.Namespace);
            Assert.AreEqual("baz", settingName.Type);
            Assert.AreEqual("qux", settingName.Property);
            Assert.AreEqual("quux", settingName.Instance);
            Assert.AreEqual("foo.bar+baz.qux,quux", settingName.ToString());
        }
    }
}
