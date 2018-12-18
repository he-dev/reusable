using System.Collections.Immutable;
using System.Configuration;
using System.Data;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionizer;
using Reusable.IOnymous;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;
using Reusable.Utilities.SqlClient;
using Xunit;

//[assembly: SettingProvider(SettingNameStrength.Medium, Prefix = "TestPrefix")]
[assembly: SettingProvider(SettingNameStrength.Low, typeof(AppSettingProvider), Prefix = "abc")]
//[assembly: SettingProvider(SettingNameStrength.Low, nameof(AppSettingProvider), Prefix = "abc")]

namespace Reusable.Tests2.SmartConfig
{
    public class FeatureTest
    {
        private static readonly SqlFourPartName SettingTableName = ("reusable", "SmartConfig");

        private static readonly IResourceProvider SettingProvider = new CompositeResourceProvider(new IResourceProvider[]
        {
            new InMemoryResourceProvider(ResourceMetadata.Empty.Add(ResourceMetadataKeys.ProviderCustomName, "Test"))
            {
                { "Test6.Member1?prefix=TestPrefix", "Value1" },
                { "Member2", "Value2" },
                { "Test7.Member", "InvalidValue1" },
            },
            new InMemoryResourceProvider(ResourceMetadata.Empty)
            {
                { "Test1.Member", "Value1" },
                { "Test2.Property", "Value2" },
                { "Test4.Property", "Value4" },
                { "Test5.Member?prefix=Prefix", "Value5" },
                { "Test7.Member", "InvalidValue2" },
            },
            new InMemoryResourceProvider(ResourceMetadata.Empty.Add(ResourceMetadataKeys.ProviderCustomName, "Test7"))
            {
                { "Test7.Member", "Value7" },
            },
            new AppSettingProvider().DecorateWith(JsonResourceProvider.Factory()),
            new SqlServerProvider("name=TestDb", ResourceMetadata.Empty) // new JsonSettingConverter() { StringTypes = new[] { typeof(string) }.ToImmutableHashSet() })
            {
                TableName = SettingTableName,
                ColumnMapping = ("_name", "_value"),
                Where = ImmutableDictionary<string, object>.Empty.Add("_other", nameof(FeatureTest))
            }.DecorateWith(JsonResourceProvider.Factory()),
        }.Select(p => p.DecorateWith(SettingProvider2.Factory())).ToArray(), ResourceMetadata.Empty);

        public FeatureTest()
        {
            SeedAppSettings();
            SeedSqlServer();
        }

        private static void SeedAppSettings()
        {
            var data = new (string Key, string Value)[]
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

        [Fact]
        public void GetValue_CanGetValueByVariousNames()
        {
            var test1 = new Test1();
            var test2 = new Test2();
            var test3 = new Test3();
            var test5 = new Test5();

            Assert.Equal("Value1", SettingProvider.GetSetting(() => test1.Member));
            Assert.Equal("Value2", SettingProvider.GetSetting(() => test2.Member));
            Assert.Equal("Value4", SettingProvider.GetSetting(() => test3.Member));
            Assert.Equal("Value5", SettingProvider.GetSetting(() => test5.Member));
        }

        [Fact]
        public void CanGetValueWithDefaultSetup()
        {
            var test1 = new Test1();
            Assert.Equal("Value1", SettingProvider.GetSetting(() => test1.Member));
        }

        [Fact]
        public void GetValue_CanGetValueWithTypeOrMemberAnnotations()
        {
            var test2 = new Test2();
            var test3 = new Test3();
            var test5 = new Test5();

            Assert.Equal("Value2", SettingProvider.GetSetting(() => test2.Member));
            Assert.Equal("Value4", SettingProvider.GetSetting(() => test3.Member));
            Assert.Equal("Value5", SettingProvider.GetSetting(() => test5.Member));
        }

        [Fact]
        public void GetValue_CanGetValueWithAssemblyAnnotations()
        {
            var test6 = new Test6();

            //Assert.Equal("Value1", SettingProvider.GetSetting(() => test6.Member1));
            Assert.Equal("Value2", SettingProvider.GetSetting(() => test6.Member2));
        }

        [Fact]
        public void GetValue_CanGetValueFromSpecificProvider()
        {
            var test7 = new Test7();

            Assert.Equal("Value7", SettingProvider.GetSetting(() => test7.Member));
        }

        [Fact]
        public void GetValue_ThrowsWhenProviderDoesNotExist()
        {
            var test8 = new Test8();

            Assert.Throws<DynamicException>(() => SettingProvider.GetSetting(() => test8.Member));//, filter => filter.When(name: "GetValue"));
        }

        [Fact]
        public void GetValue_CanReadAndWriteWithSqlServer()
        {
            var test = new Test9 { Configuration = SettingProvider };

            Assert.Equal("Hallo!", test.Greeting);

            test.Greeting = "Yo!";

            Assert.Equal("Yo!", test.Greeting);
        }

        [Fact]
        public void GetValue_CanReadFromAppConfig()
        {
            var test = new Test9 { Configuration = SettingProvider };

            Assert.Equal("Hi!", test.Salute);
        }
    }

    internal class Test0
    {
        public IResourceProvider Configuration { get; set; }
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
        [SettingMember(ProviderType = typeof(AppSettingProvider))]
        public string Salute => Configuration.GetSetting(() => Salute);

        [SettingMember(ProviderType = typeof(SqlServerProvider))]
        public string Greeting
        {
            get => Configuration.GetSetting(() => Greeting);
            set => Configuration.SetSetting(() => Greeting, value);
        }
    }
}