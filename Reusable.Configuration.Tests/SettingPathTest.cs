using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public class SettingPathTest
    {
        [TestMethod]
        public void Parse_ShortWeak_TwoNames()
        {
            var settingPath = SettingPath.Parse("Foo.Bar");
            settingPath.ConsumerNamespace.Verify().IsEmpty();
            settingPath.ConsumerName.Verify().IsNullOrEmpty();
            settingPath.InstanceName.Verify().IsNullOrEmpty();
            settingPath.ContainerName.Verify().IsEqual("Foo");
            settingPath.SettingName.Verify().IsEqual("Bar");
            settingPath.ElementName.Verify().IsNullOrEmpty();

        }

        [TestMethod]
        public void Parse_ShortStrong_TwoNamesAndKey()
        {
            var settingPath = SettingPath.Parse("Foo.Bar[\"Baz\"]");
            settingPath.ConsumerNamespace.Verify().IsEmpty();
            settingPath.ConsumerName.Verify().IsNullOrEmpty();
            settingPath.InstanceName.Verify().IsNullOrEmpty();
            settingPath.ContainerName.Verify().IsEqual("Foo");
            settingPath.SettingName.Verify().IsEqual("Bar");
            settingPath.ElementName.Verify().IsEqual("\"Baz\"");
        }

        [TestMethod]
        public void Parse_FullWeak_ConsumerNamesAndContainerNames()
        {
            var settingPath = SettingPath.Parse("abc.jkl.xyz[Any].Foo.Bar");
            settingPath.ConsumerNamespace.Verify().SequenceEqual(new[] { "abc", "jkl" });
            settingPath.ConsumerName.Verify().IsEqual("xyz");
            settingPath.InstanceName.Verify().IsNullOrEmpty();
            settingPath.ContainerName.Verify().IsEqual("Foo");
            settingPath.SettingName.Verify().IsEqual("Bar");
            settingPath.ElementName.Verify().IsNullOrEmpty();
        }

        [TestMethod]
        public void Parse_FullStrong_ConsumerNamesAndContainerNamesAndKey()
        {
            var settingPath = SettingPath.Parse("abc.jkl.xyz[Any].Foo.Bar[\"Baz\"]");
            settingPath.ConsumerNamespace.Verify().SequenceEqual(new[] { "abc", "jkl" });
            settingPath.ConsumerName.Verify().IsEqual("xyz");
            settingPath.InstanceName.Verify().IsNullOrEmpty();
            settingPath.ContainerName.Verify().IsEqual("Foo");
            settingPath.SettingName.Verify().IsEqual("Bar");
            settingPath.ElementName.Verify().IsEqual("\"Baz\"");
        }

        [TestMethod]
        public void Parse_FullStrongWithName_ConsumerNamesConsumerNameAndContainerNamesAndKey()
        {
            var settingPath = SettingPath.Parse("abc.jkl.xyz[\"qwe\"].Foo.Bar[\"Baz\"]");
            settingPath.ConsumerNamespace.Verify().SequenceEqual(new[] { "abc", "jkl" });
            settingPath.ConsumerName.Verify().IsEqual("xyz");
            settingPath.InstanceName.Verify().IsEqual("\"qwe\"");
            settingPath.ContainerName.Verify().IsEqual("Foo");
            settingPath.SettingName.Verify().IsEqual("Bar");
            settingPath.ElementName.Verify().IsEqual("\"Baz\"");
        }

        [TestMethod]
        public void ToString_ShortWeak_SettingPath()
        {
            var settingPath = SettingPath.Parse(@"Foo.Bar");
            settingPath.ToString(SettingPathFormat.ShortWeak, SettingPathFormatter.Instance).Verify().IsEqual("Foo.Bar");
        }

        [TestMethod]
        public void ToString_ShortStrong_SettingPath()
        {
            var settingPath = SettingPath.Parse(@"Foo.Bar[""Baz""]");
            settingPath.ToString(SettingPathFormat.ShortStrong, SettingPathFormatter.Instance).Verify().IsEqual(@"Foo.Bar[""Baz""]");
        }

        [TestMethod]
        public void ToString_FullWeak_SettingPath()
        {
            var settingPath = SettingPath.Parse(@"abc.jkl.xyz[Any].Foo.Bar");
            settingPath.ToString(SettingPathFormat.FullWeak, SettingPathFormatter.Instance).Verify().IsEqual(@"abc.jkl.xyz[Any].Foo.Bar");
        }

        [TestMethod]
        public void ToString_FullStrong_SettingPath()
        {
            var settingPath = SettingPath.Parse(@"abc.jkl.xyz[Any].Foo.Barr[""Baz""]");
            settingPath.ToString(SettingPathFormat.FullStrong, SettingPathFormatter.Instance).Verify().IsEqual(@"abc.jkl.xyz[Any].Foo.Barr[""Baz""]");
        }

        [TestMethod]
        public void ToString_FullStrongWithInstanceName_SettingPath()
        {
            var settingPath = SettingPath.Parse(@"abc.jkl.xyz[""qwe""].Foo.Barr[""Baz""]");
            settingPath.ToString(SettingPathFormat.FullStrong, SettingPathFormatter.Instance).Verify().IsEqual(@"abc.jkl.xyz[""qwe""].Foo.Barr[""Baz""]");
        }
    }
}
