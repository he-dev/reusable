using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Tests.Data
{
    [TestClass]
    public class SettingNameTest
    {
        // foo.bar+baz.qux,quux

        [TestMethod]
        public void ctor_Property_OtherNull()
        {
            var sn = new SettingName("foo");
            Assert.IsNull(sn.Namespace);
            Assert.IsNull(sn.Type);
            Assert.AreEqual("foo", sn.Property.ToString());
            Assert.IsNull(sn.Instance);
        }

        [TestMethod]
        public void copyCtor_SettingName_SettingName()
        {
            var sn = new SettingName(new SettingName("qux")
            {
                Namespace = "foo.bar",
                Type = "baz",
                Instance = "quux"
            });
            Assert.AreEqual("foo.bar", sn.Namespace.ToString());
            Assert.AreEqual("baz", sn.Type.ToString());
            Assert.AreEqual("qux", sn.Property.ToString());
            Assert.AreEqual("quux", sn.Instance.ToString());
        }

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

        [TestMethod]
        public void Equals_SameValues_True()
        {
            Assert.AreEqual(
                SettingName.Parse("foo.bar+baz.qux,quux"), 
                SettingName.Parse("foo.bar+baz.qux,quux"));

            Assert.AreEqual(
                SettingName.Parse("baz.qux,quux"),
                SettingName.Parse("baz.qux,quux"));

            Assert.AreEqual(
                SettingName.Parse("qux,quux"),
                SettingName.Parse("qux,quux"));

            Assert.AreEqual(
                SettingName.Parse("foo.bar+baz.qux"),
                SettingName.Parse("foo.bar+baz.qux"));

            Assert.AreEqual(
                SettingName.Parse("qux"),
                SettingName.Parse("qux"));
        }

        [TestMethod]
        public void Equals_DifferentValues_False()
        {
            Assert.AreNotEqual(
                SettingName.Parse("foo.bar+baz.qux,quux"),
                SettingName.Parse("foo.bar+baz.qux"));            
        }
    }
}
