using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Annotations;
using Reusable.Utilities.MSTest;

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
                { "Test7.Member", "InvalidValue1" },
            },
            new InMemory(new RelaySettingConverter())
            {
                { "Test1.Member", "Value1" },
                { "Test2.Property", "Value2" },
                { "Test4.Property", "Value4" },
                { "Prefix:Test5.Member", "Value5" },
                { "Test7.Member", "InvalidValue2" },
            },
            new InMemory(new RelaySettingConverter(), "Test7")
            {
                { "Test7.Member", "Value7" },
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
        public void CanGetValueWithTypeOrMemberAnnotations()
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
        
        [TestMethod]
        public void CanGetValueFromSpecificProvider()
        {
            var test7 = new Test7();

            Assert.AreEqual("Value7", Configuration.GetValue(() => test7.Member));
        }
        
        [TestMethod]
        public void ThrowsWhenProviderDoesNotExist()
        {
            var test8 = new Test8();

            Assert.That.Throws<DynamicException>(() => Configuration.GetValue(() => test8.Member), filter => filter.When(name: "GetValue"));
        }
    }

    // tests defaults
    internal class Test1
    {
        public string Member { get; set; }
    }

    // tests annotations
    internal class Test2
    {
        [SettingMember(Name = "Property")]
        public string Member { get; set; }
    }

    // tests annotations
    [SettingType(Name = "Test4")]
    internal class Test3
    {
        [SettingMember(Name = "Property")]
        public string Member { get; set; }
    }

    // tests annotations
    [SettingType(Prefix = "Prefix")]
    internal class Test5
    {
        public string Member { get; set; }
    }
    
    // tests assembly annotations
    internal class Test6
    {
        public string Member1 { get; set; }
        
        [SettingMember(Complexity = SettingNameComplexity.Low, PrefixHandling = PrefixHandling.Disable)]
        public string Member2 { get; set; }
    }

    // changes provider resolution behavior
    internal class Test7
    {
        [SettingMember(ProviderName = "Test7")]
        public string Member { get; set; }
    }
    
    // provider does not exist
    internal class Test8
    {
        [SettingMember(ProviderName = "Test8")]
        public string Member { get; set; }
    }
}