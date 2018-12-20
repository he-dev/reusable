using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Exceptionizer;
using Reusable.Reflection;
using Reusable.SmartConfig;
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
            var sn = SettingIdentifier.Parse("foo");
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
            Assert.That.Throws<DynamicException>(() => SettingIdentifier.Parse("foo+bar+baz"), filter => filter.When(name: "^SettingNameFormat"));
        }

        [TestMethod]
        public void Parse_CanReadMember()
        {
            var settingName = SettingIdentifier.Parse("qux");

            Assert.That.IsNullOrEmpty(settingName.Prefix);
            Assert.That.IsNullOrEmpty(settingName.Namespace);
            Assert.That.IsNullOrEmpty(settingName.Type);
            Assert.AreEqual("qux", settingName.Member);
            Assert.That.IsNullOrEmpty(settingName.Instance);
            Assert.AreEqual("qux", settingName.ToString());
        }

        [TestMethod]
        public void Parse_CanReadTypeAndMember()
        {
            var settingName = SettingIdentifier.Parse("baz.qux");

            Assert.That.IsNullOrEmpty(settingName.Namespace);
            Assert.AreEqual("baz", settingName.Type);
            Assert.AreEqual("qux", settingName.Member);
            Assert.That.IsNullOrEmpty(settingName.Instance);
            Assert.AreEqual("baz.qux", settingName.ToString());
        }

        [TestMethod]
        public void Parse_CanReadNamespaceTypeAndMember()
        {
            var settingName = SettingIdentifier.Parse("foo.bar+baz.qux");

            Assert.AreEqual("foo.bar", settingName.Namespace);
            Assert.AreEqual("baz", settingName.Type);
            Assert.AreEqual("qux", settingName.Member);
            Assert.That.IsNullOrEmpty(settingName.Instance);
            Assert.AreEqual("foo.bar+baz.qux", settingName.ToString());
        }

        [TestMethod]
        public void Parse_CanReadNamespaceTypeMemberAndInstance()
        {
            var settingName = SettingIdentifier.Parse("foo.bar+baz.qux,quux");

            Assert.AreEqual("foo.bar", settingName.Namespace);
            Assert.AreEqual("baz", settingName.Type);
            Assert.AreEqual("qux", settingName.Member);
            Assert.AreEqual("quux", settingName.Instance);
            Assert.AreEqual("foo.bar+baz.qux,quux", settingName.ToString());
        }
        
        [TestMethod]
        public void Parse_CanReadAssemblyNamespaceTypeMemberAndInstance()
        {
            var settingName = SettingIdentifier.Parse("Assem.bly:Name.space+Type.Member,Instance");

            Assert.AreEqual("Assem.bly", settingName.Prefix);
            Assert.AreEqual("Name.space", settingName.Namespace);
            Assert.AreEqual("Type", settingName.Type);
            Assert.AreEqual("Member", settingName.Member);
            Assert.AreEqual("Instance", settingName.Instance);
            Assert.AreEqual("Assem.bly:Name.space+Type.Member,Instance", settingName.ToString());
        }

        [TestMethod]
        public void Equals_CanCompareSimilarObjects()
        {
            Assert.AreEqual(
                SettingIdentifier.Parse("foo.bar+baz.qux,quux"), 
                SettingIdentifier.Parse("foo.bar+baz.qux,quux"));

            Assert.AreEqual(
                SettingIdentifier.Parse("baz.qux,quux"),
                SettingIdentifier.Parse("baz.qux,quux"));

            Assert.AreEqual(
                SettingIdentifier.Parse("qux,quux"),
                SettingIdentifier.Parse("qux,quux"));

            Assert.AreEqual(
                SettingIdentifier.Parse("foo.bar+baz.qux"),
                SettingIdentifier.Parse("foo.bar+baz.qux"));

            Assert.AreEqual(
                SettingIdentifier.Parse("qux"),
                SettingIdentifier.Parse("qux"));
        }

        [TestMethod]
        public void Equals_CanCompareDifferentObjects()
        {
            Assert.AreNotEqual(
                SettingIdentifier.Parse("foo.bar+baz.qux,quux"),
                SettingIdentifier.Parse("foo.bar+baz.qux"));            
        }
    }
}
