using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data;
using Reusable.Exceptionizer;
using Reusable.Reflection;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Annotations;
using Reusable.Utilities.MSTest;
using Reusable.Utilities.SqlClient;
using Configuration = Reusable.SmartConfig.Configuration;

[assembly: SettingProvider("Test", Prefix = "TestPrefix")]
[assembly: SettingProvider(typeof(AppSettings), Prefix = "abc", SettingNameStrength = SettingNameStrength.Low)]

namespace Reusable.Tests.SmartConfig.Integration
{
    [TestClass]
    public class FeatureTest
    {
        private static readonly SqlFourPartName SettingTableName = ("reusable", "SmartConfig");

        private static readonly IConfiguration Configuration = new Configuration(new ISettingProvider[]
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
            new AppSettings(new JsonSettingConverter()),
            new SqlServer("name=TestDb", new JsonSettingConverter() { StringTypes = new[] { typeof(string) }.ToImmutableHashSet() })
            {
                SettingTableName = SettingTableName,
                ColumnMapping = ("_name", "_value"),
                Where = new Dictionary<string, object>
                {
                    ["_other"] = nameof(FeatureTest)
                }
            },
        });

        [TestInitialize]
        public void TestInitialize()
        {
            SeedAppSettings();
            SeedSqlServer();
        }

        private static void SeedAppSettings()
        {
            var data = new(string Key, string Value)[]
            {
                ("abc:Salute", "Hi!"),
            };

            var exeConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            exeConfiguration.AppSettings.Settings.Clear();
            //exeConfiguration.ConnectionStrings.ConnectionStrings.Clear();

            foreach (var (key, value) in data)
            {
                exeConfiguration.AppSettings.Settings.Add(key, value);
            }

            exeConfiguration.Save(ConfigurationSaveMode.Minimal);
        }

        private static void SeedSqlServer()
        {
            var data =
                    new DataTable()
                        .AddColumn("_name", typeof(string))
                        .AddColumn("_value", typeof(string))
                        .AddColumn("_other", typeof(string))
                        .AddRow("Test9.Greeting", "Hallo!", nameof(FeatureTest))
//                        .AddRow("MockClass2.Bar", "text", "integration")
//                        .AddRow("TestClass3.Baz", "1.23", "integration")
//                        // Used for exception testing.
//                        .AddRow("TestClass3.Qux", "quux", "integration")
//                        .AddRow($"{Namespace}+TestClass3.Qux", "quux", "integration")
                ;

            SqlHelper.Execute("name=TestDb", connection => { connection.Seed(SettingTableName, data); });
        }

        [TestMethod]
        public void GetValue_CanGetValueByVariousNames()
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
        public void GetValue_CanGetValueWithTypeOrMemberAnnotations()
        {
            var test2 = new Test2();
            var test3 = new Test3();
            var test5 = new Test5();

            Assert.AreEqual("Value2", Configuration.GetValue(() => test2.Member));
            Assert.AreEqual("Value4", Configuration.GetValue(() => test3.Member));
            Assert.AreEqual("Value5", Configuration.GetValue(() => test5.Member));
        }

        [TestMethod]
        public void GetValue_CanGetValueWithAssemblyAnnotations()
        {
            var test6 = new Test6();

            Assert.AreEqual("Value1", Configuration.GetValue(() => test6.Member1));
            Assert.AreEqual("Value2", Configuration.GetValue(() => test6.Member2));
        }

        [TestMethod]
        public void GetValue_CanGetValueFromSpecificProvider()
        {
            var test7 = new Test7();

            Assert.AreEqual("Value7", Configuration.GetValue(() => test7.Member));
        }

        [TestMethod]
        public void GetValue_ThrowsWhenProviderDoesNotExist()
        {
            var test8 = new Test8();

            Assert.That.Throws<DynamicException>(() => Configuration.GetValue(() => test8.Member), filter => filter.When(name: "GetValue"));
        }

        [TestMethod]
        public void GetValue_CanReadAndWriteWithSqlServer()
        {
            var test = new Test9 { Configuration = Configuration };

            Assert.AreEqual("Hallo!", test.Greeting);
            
            test.Greeting = "Yo!";
            
            Assert.AreEqual("Yo!", test.Greeting);
        }

        [TestMethod]
        public void GetValue_CanReadFromAppConfig()
        {
            var test = new Test9 { Configuration = Configuration };

            Assert.AreEqual("Hi!", test.Salute);
        }
    }

    internal class Test0
    {
        public IConfiguration Configuration { get; set; }
    }

    // tests defaults
    internal class Test1 : Test0
    {
        public string Member { get; set; }
    }

    // tests annotations
    internal class Test2 : Test0
    {
        [SettingMember(Name = "Property")]
        public string Member { get; set; }
    }

    // tests annotations
    [SettingType(Name = "Test4")]
    internal class Test3 : Test0
    {
        [SettingMember(Name = "Property")]
        public string Member { get; set; }
    }

    // tests annotations
    [SettingType(Prefix = "Prefix")]
    internal class Test5 : Test0
    {
        public string Member { get; set; }
    }

    // tests assembly annotations
    internal class Test6 : Test0
    {
        public string Member1 { get; set; }

        [SettingMember(Strength = SettingNameStrength.Low, PrefixHandling = PrefixHandling.Disable)]
        public string Member2 { get; set; }
    }

    // changes provider resolution behavior
    internal class Test7 : Test0
    {
        [SettingMember(ProviderName = "Test7")]
        public string Member { get; set; }
    }

    // provider does not exist
    internal class Test8 : Test0
    {
        [SettingMember(ProviderName = "Test8")]
        public string Member { get; set; }
    }

    // tests app-config and sql-server
    internal class Test9 : Test0
    {
        [SettingMember(ProviderType = typeof(AppSettings))]
        public string Salute => Configuration.GetValue(() => Salute);

        [SettingMember(ProviderType = typeof(SqlServer))]
        public string Greeting
        {
            get => Configuration.GetValue(() => Greeting);
            set => Configuration.SetValue(() => Greeting, value);
        }
    }
}