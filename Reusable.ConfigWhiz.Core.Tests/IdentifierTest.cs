using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Paths;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public class IdentifierTest
    {
        [TestMethod]
        public void Parse_ContainerSetting_Identifier()
        {
            Assert.IsTrue(new Identifier(Enumerable.Empty<string>(), null, null, "Foo", "Bar", null, IdentifierLength.Medium).Equals(Identifier.Parse("Foo.Bar")));
        }

        [TestMethod]
        public void Parse_ShortStrong_TwoNamesAndKey()
        {
            var settingPath = Identifier.Parse("Foo.Bar[\"Baz\"]");
            settingPath.Context.Verify().IsEmpty();
            settingPath.Consumer.Verify().IsNullOrEmpty();
            settingPath.Instance.Verify().IsNullOrEmpty();
            settingPath.Container.Verify().IsEqual("Foo");
            settingPath.Setting.Verify().IsEqual("Bar");
            settingPath.Element.Verify().IsEqual("Baz");
        }

        [TestMethod]
        public void Parse_FullWeak_ConsumerNamesAndContainerNames()
        {
            var settingPath = Identifier.Parse("abc.jkl.xyz.Foo.Bar");
            settingPath.Context.Verify().SequenceEqual(new[] { "abc", "jkl" });
            settingPath.Consumer.Verify().IsEqual("xyz");
            settingPath.Instance.Verify().IsNullOrEmpty();
            settingPath.Container.Verify().IsEqual("Foo");
            settingPath.Setting.Verify().IsEqual("Bar");
            settingPath.Element.Verify().IsNullOrEmpty();
        }

        [TestMethod]
        public void Parse_FullStrong_ConsumerNamesAndContainerNamesAndKey()
        {
            var settingPath = Identifier.Parse("abc.jkl.xyz.Foo.Bar[\"Baz\"]");
            settingPath.Context.Verify().SequenceEqual(new[] { "abc", "jkl" });
            settingPath.Consumer.Verify().IsEqual("xyz");
            settingPath.Instance.Verify().IsNullOrEmpty();
            settingPath.Container.Verify().IsEqual("Foo");
            settingPath.Setting.Verify().IsEqual("Bar");
            settingPath.Element.Verify().IsEqual("Baz");
        }

        [TestMethod]
        public void Parse_FullStrongWithName_ConsumerNamesConsumerNameAndContainerNamesAndKey()
        {
            var settingPath = Identifier.Parse("abc.jkl.xyz[\"qwe\"].Foo.Bar[\"Baz\"]");
            settingPath.Context.Verify().SequenceEqual(new[] { "abc", "jkl" });
            settingPath.Consumer.Verify().IsEqual("xyz");
            settingPath.Instance.Verify().IsEqual("qwe");
            settingPath.Container.Verify().IsEqual("Foo");
            settingPath.Setting.Verify().IsEqual("Bar");
            settingPath.Element.Verify().IsEqual("Baz");
        }

        [TestMethod]
        public void Parse_FullStrongWithUnqotedName_Success()
        {
            var settingPath = Identifier.Parse("abc.jkl.xyz.Foo.Bar[Baz]");
            settingPath.Context.Verify().SequenceEqual(new[] { "abc", "jkl" });
            settingPath.Consumer.Verify().IsEqual("xyz");
            settingPath.Instance.Verify().IsNullOrEmpty();
            settingPath.Container.Verify().IsEqual("Foo");
            settingPath.Setting.Verify().IsEqual("Bar");
            settingPath.Element.Verify().IsEqual("Baz");
        }

        [TestMethod]
        public void ToString_Short_Setting()
        {
            var settingPath = Identifier.Parse(@"Bar");
            settingPath.ToString($".{IdentifierLength.Short}", IdentifierFormatter.Instance).Verify().IsEqual("Bar");
        }

        [TestMethod]
        public void ToString_ShortWithElement_SettingElement()
        {
            var settingPath = Identifier.Parse(@"Bar[""Baz""]");
            settingPath.ToString($".{IdentifierLength.Short}", IdentifierFormatter.Instance).Verify().IsEqual(@"Bar[""Baz""]");
        }

        [TestMethod]
        public void ToString_Medium_ContainerSetting()
        {
            var settingPath = Identifier.Parse(@"Container.Setting");
            settingPath.ToString($".{IdentifierLength.Medium}", IdentifierFormatter.Instance).Verify().IsEqual(@"Container.Setting");
        }

        [TestMethod]
        public void ToString_Long_ConsumerContainerSetting()
        {
            var settingPath = Identifier.Parse(@"Consumer.Container.Setting");
            settingPath.ToString($".{IdentifierLength.Long}", IdentifierFormatter.Instance).Verify().IsEqual(@"Consumer.Container.Setting");
        }

        [TestMethod]
        public void ToString_Unique_NamespaceConsumerContainerSetting()
        {
            var settingPath = Identifier.Parse(@"Namespace.Consumer.Container.Setting");
            settingPath.ToString($".{IdentifierLength.Unique}", IdentifierFormatter.Instance).Verify().IsEqual(@"Namespace.Consumer.Container.Setting");
        }

        [TestMethod]
        public void Equals_SameProperties_True()
        {
            var identifier1 = new Identifier(Enumerable.Empty<string>(), "Consumer", "Instance", "Container", "Setting", "Element", IdentifierLength.Long);
            var identifier2 = new Identifier(Enumerable.Empty<string>(), "Consumer", "Instance", "Container", "Setting", "Element", IdentifierLength.Long);
            Assert.IsTrue(identifier1 == identifier2);
            Assert.IsTrue(identifier1 == identifier1);
        }

        [TestMethod]
        public void Equals_DifferentProperties_False()
        {
            var identifier1 = new Identifier(Enumerable.Empty<string>(), "Consumer1", "Instance1", "Container1", "Setting1", "Element1", IdentifierLength.Long);
            var identifier2 = new Identifier(Enumerable.Empty<string>(), "Consumer2", "Instance2", "Container2", "Setting2", "Element2", IdentifierLength.Long);
            Assert.IsFalse(identifier1 == identifier2);
        }

    }
}
