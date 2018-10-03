using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;
using Reusable.SmartConfig.Data;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.SmartConfig.Data
{
    [TestClass]
    public class SettingNameTest
    {
        // foo.bar+baz.qux,quux

        [TestMethod]
        public void ctor_CanInstantiateFromMember()
        {
            var sn = SettingName.Parse("foo");
            Assert.That.IsNullOrEmpty(sn.Namespace);
            Assert.That.IsNullOrEmpty(sn.Type);
            Assert.AreEqual("foo", sn.Member);
            Assert.That.IsNullOrEmpty(sn.Instance);
        }

//        [TestMethod]
//        public void ctor_CanInstantiateFromOtherInstance()
//        {
//            var sn = new SettingName(new SettingName("qux")
//            {
//                Namespace = "foo.bar",
//                Type = "baz",
//                Instance = "quux"
//            });
//            Assert.AreEqual("foo.bar", sn.Namespace.ToString());
//            Assert.AreEqual("baz", sn.Type.ToString());
//            Assert.AreEqual("qux", sn.Member.ToString());
//            Assert.AreEqual("quux", sn.Instance.ToString());
//        }

        [TestMethod]
        public void Parse_DisallowsInvalidFormat()
        {
            Assert.That.Throws<DynamicException>(() => SettingName.Parse("foo+bar+baz"), filter => filter.When(name: "^SettingNameFormat"));
        }

        [TestMethod]
        public void Parse_CanReadMember()
        {
            var settingName = SettingName.Parse("qux");

            Assert.That.IsNullOrEmpty(settingName.Assembly);
            Assert.That.IsNullOrEmpty(settingName.Namespace);
            Assert.That.IsNullOrEmpty(settingName.Type);
            Assert.AreEqual("qux", settingName.Member);
            Assert.That.IsNullOrEmpty(settingName.Instance);
            Assert.AreEqual("qux", settingName.ToString());
        }

        [TestMethod]
        public void Parse_CanReadTypeAndMember()
        {
            var settingName = SettingName.Parse("baz.qux");

            Assert.That.IsNullOrEmpty(settingName.Namespace);
            Assert.AreEqual("baz", settingName.Type);
            Assert.AreEqual("qux", settingName.Member);
            Assert.That.IsNullOrEmpty(settingName.Instance);
            Assert.AreEqual("baz.qux", settingName.ToString());
        }

        [TestMethod]
        public void Parse_CanReadNamespaceTypeAndMember()
        {
            var settingName = SettingName.Parse("foo.bar+baz.qux");

            Assert.AreEqual("foo.bar", settingName.Namespace);
            Assert.AreEqual("baz", settingName.Type);
            Assert.AreEqual("qux", settingName.Member);
            Assert.That.IsNullOrEmpty(settingName.Instance);
            Assert.AreEqual("foo.bar+baz.qux", settingName.ToString());
        }

        [TestMethod]
        public void Parse_CanReadNamespaceTypeMemberAndInstance()
        {
            var settingName = SettingName.Parse("foo.bar+baz.qux,quux");

            Assert.AreEqual("foo.bar", settingName.Namespace);
            Assert.AreEqual("baz", settingName.Type);
            Assert.AreEqual("qux", settingName.Member);
            Assert.AreEqual("quux", settingName.Instance);
            Assert.AreEqual("foo.bar+baz.qux,quux", settingName.ToString());
        }

        [TestMethod]
        public void Equals_CanCompareSimilarObjects()
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
        public void Equals_CanCompareDifferentObjects()
        {
            Assert.AreNotEqual(
                SettingName.Parse("foo.bar+baz.qux,quux"),
                SettingName.Parse("foo.bar+baz.qux"));            
        }
    }
}
