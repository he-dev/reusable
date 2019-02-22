using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Data;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionizer;
using Reusable.IOnymous;
using Reusable.SmartConfig;
using Reusable.Utilities.SqlClient;
using Xunit;

//[assembly: SettingProvider(SettingNameStrength.Medium, Prefix = "TestPrefix")]
[assembly: SettingProvider(SettingNameStrength.Low, typeof(AppSettingProvider), Prefix = "abc")]
//[assembly: SettingProvider(SettingNameStrength.Low, nameof(AppSettingProvider), Prefix = "abc")]

namespace Reusable.Tests.XUnit.SmartConfig
{
    public class UseCaseTest
    {
        private static readonly SqlFourPartName SettingTableName = ("reusable", "SmartConfig");

        private readonly IConfiguration _configuration;

        public UseCaseTest()
        {
            _configuration =
                new Reusable.SmartConfig.Configuration(
                    new CompositeProvider(new IResourceProvider[]
                    {
                        new InMemoryProvider(ResourceMetadata.Empty.ProviderCustomName("Memory1"))
                        {
                            { "setting:Test6.Member1?prefix=TestPrefix", "Value1" },
                            { "setting:Member2", "Value2" },
                            { "setting:Test7.Member", "InvalidValue1" },
                        },
                        new InMemoryProvider(ResourceMetadata.Empty.ProviderCustomName("Memory2"))
                        {
                            { "setting:Test1.Member", "Value1" },
                            { "setting:Test2.Property", "Value2" },
                            { "setting:Test4.Property", "Value4" },
                            { "setting:Test5.Member?prefix=Prefix", "Value5" },
                            { "setting:Test7.Member", "InvalidValue2" },
                        },
                        new InMemoryProvider(ResourceMetadata.Empty.ProviderCustomName("Memory3"))
                        {
                            { "setting:Test7.Member", "Value7" },
                        },
                        new AppSettingProvider(new UriStringToSettingIdentifierConverter()),
                        new SqlServerProvider("name=TestDb", new UriStringToSettingIdentifierConverter())
                        {
                            TableName = SettingTableName,
                            ColumnMappings =
                                ImmutableDictionary<SqlServerColumn, SoftString>
                                    .Empty
                                    .Add(SqlServerColumn.Name, "_name")
                                    .Add(SqlServerColumn.Value, "_value"),
                            Where = ImmutableDictionary<string, object>.Empty.Add("_other", nameof(UseCaseTest))
                        },
                    }.Select(p => p.DecorateWith(Reusable.SmartConfig.SettingNameProvider.Factory())))
                );
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
                    .AddRow("Test9.Greeting", "Hallo!", nameof(UseCaseTest));

            using (data)
            {
                SqlHelper.Execute("name=TestDb", connection => { connection.Seed(SettingTableName, data); });
            }
        }

        [Fact]
        public void Can_get_settings_by_default_convention()
        {
            var test1 = new Test1();
            Assert.Equal("Value1", _configuration.GetSetting(() => test1.Member));
        }

        [Fact]
        public void Can_get_settings_with_type_or_member_annotations()
        {
            var test2 = new Test2();
            var test3 = new Test3();
            var test5 = new Test5();

            Assert.Equal("Value2", _configuration.GetSetting(() => test2.Member));
            Assert.Equal("Value4", _configuration.GetSetting(() => test3.Member));
            Assert.Equal("Value5", _configuration.GetSetting(() => test5.Member));
        }

        [Fact]
        public void Can_get_settings_with_assembly_annotations()
        {
            var test6 = new Test6();

            //Assert.Equal("Value1", SettingProvider.GetSetting(() => test6.Member1));
            Assert.Equal("Value2", _configuration.GetSetting(() => test6.Member2));
        }

        [Fact]
        public void Can_get_settings_from_specific_provider()
        {
            var test7 = new Test7();

            Assert.Equal("Value7", _configuration.GetSetting(() => test7.Member));
        }

        [Fact]
        public void Throws_when_setting_does_not_exist()
        {
            var test8 = new Test8();

            Assert.ThrowsAny<DynamicException>(() => _configuration.GetSetting(() => test8.Member));
        }

        [Fact]
        public void Can_get_and_put_settings_with_SqlServerProvider()
        {
            var test = new Test9 { Configuration = _configuration };

            Assert.Equal("Hallo!", test.Greeting);

            test.Greeting = "Yo!";

            Assert.Equal("Yo!", test.Greeting);
        }

        [Fact]
        public void Can_get_and_put_settings_with_AppConfigProvider()
        {
            var test = new Test9 { Configuration = _configuration };

            Assert.Equal("Hi!", test.Salute);
        }

        [Fact]
        public void Can_deserialize_TimeSpan()
        {
            var settingProviders = new IResourceProvider[]
                {
                    new InMemoryProvider(ResourceMetadata.Empty.ProviderCustomName("Memory1"))
                    {
                        { $"setting:{nameof(CustomTypes)}.{nameof(CustomTypes.TimeSpan)}", "\"00:20:00\"" },
                    }
                }
                .Select(p => p.DecorateWith(Reusable.SmartConfig.SettingNameProvider.Factory()));

            var configuration = new Reusable.SmartConfig.Configuration(new CompositeProvider(settingProviders));
            var customTypes = new CustomTypes { Configuration = configuration };
            Assert.Equal(customTypes.TimeSpan, TimeSpan.FromMinutes(20));
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
        [SettingMember(ProviderName = "Memory3")]
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
            set => Configuration.SaveSetting(() => Greeting, value);
        }
    }

    internal class CustomTypes : Test0
    {
        public TimeSpan TimeSpan => Configuration.GetSetting(() => TimeSpan);
    }
}