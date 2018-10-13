using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Annotations;

[assembly: SettingProvider("Test", Prefix = "TestPrefix")]

namespace Reusable.Tests.SmartConfig.Integration
{
    [TestClass]
    public class FeatureTest
    {
        private static readonly IConfiguration Configuration = new Configuration(new[]
        {
            new InMemory(new RelaySettingConverter(), "Test")
            {
                { "TestPrefix:Test6.Member1", "Value1" },
                { "Member2", "Value2" },
            },
            new InMemory(new RelaySettingConverter())
            {
                { "Test1.Member", "Value1" },
                { "Test2.Property", "Value2" },
                { "Test4.Property", "Value4" },
                { "Prefix:Test5.Member", "Value5" },
            },
        });

        [TestMethod]
        public void CanGetValueByVariousNames()
        {
            var test1 = new Test1();
            var test2 = new Test2();
            var test3 = new Test3();
            var test5 = new Test5();

            Assert.AreEqual("Value1", Configuration.GetValue(() => test1.Member));
            Assert.AreEqual("Value2", Configuration.GetValue(() => test2.Member));
            Assert.AreEqual("Value4", Configuration.GetValue(() => test3.Member));
            Assert.AreEqual("Value5", Configuration.GetValue(() => test5.Member));
        }

        [TestMethod]
        public void CanGetValueWithDefaultSetup()
        {
            var test1 = new Test1();
            Assert.AreEqual("Value1", Configuration.GetValue(() => test1.Member));
        }

        [TestMethod]
        public void CanGetValueWithAnnotations()
        {
            var test2 = new Test2();
            var test3 = new Test3();
            var test5 = new Test5();

            Assert.AreEqual("Value2", Configuration.GetValue(() => test2.Member));
            Assert.AreEqual("Value4", Configuration.GetValue(() => test3.Member));
            Assert.AreEqual("Value5", Configuration.GetValue(() => test5.Member));
        }
        
        [TestMethod]
        public void CanGetValueWithAssemblyAnnotations()
        {
            var test6 = new Test6();

            Assert.AreEqual("Value1", Configuration.GetValue(() => test6.Member1));
            Assert.AreEqual("Value2", Configuration.GetValue(() => test6.Member2));
        }
    }

    internal class Test1
    {
        public string Member { get; set; }
    }

    internal class Test2
    {
        [SettingMember(Name = "Property")]
        public string Member { get; set; }
    }

    [SettingType(Name = "Test4")]
    internal class Test3
    {
        [SettingMember(Name = "Property")]
        public string Member { get; set; }
    }

    [SettingType(Prefix = "Prefix")]
    internal class Test5
    {
        public string Member { get; set; }
    }
    
    internal class Test6
    {
        public string Member1 { get; set; }
        
        [SettingMember(Complexity = SettingNameComplexity.Low, PrefixHandling = PrefixHandling.Disable)]
        public string Member2 { get; set; }
    }
}