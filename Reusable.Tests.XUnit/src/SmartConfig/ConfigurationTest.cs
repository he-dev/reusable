using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Data;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.IOnymous;
using Reusable.SmartConfig;
using Reusable.Utilities.SqlClient;
using Xunit;
using cfg = Reusable.SmartConfig.Configuration;

//[assembly: SettingProvider(SettingNameStrength.Medium, Prefix = "TestPrefix")]
//[assembly: SettingProvider(SettingNameStrength.Low, typeof(AppSettingProvider), Prefix = "abc")]
//[assembly: SettingProvider(SettingNameStrength.Low, nameof(AppSettingProvider), Prefix = "abc")]

/*
 
  - can find setting by name
  - can find first setting by name
  - can find setting by name and provider
  - can find setting by name and provider-type 
  - can override type-name
  - can override member-name
  - can override setting name length
  - can add setting name prefix
  - can disable setting name prefix
  - can find setting for base type
  
 */
namespace Reusable.Tests.XUnit.SmartConfig
{
    public class ConfigurationTest
    {
        private static readonly SqlFourPartName SettingTableName = ("reusable", "SmartConfig");

        private readonly IConfiguration _configuration;

        public ConfigurationTest()
        {
            _configuration = new Reusable.SmartConfig.Configuration
            (
                new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(ResourceMetadata.Empty.CustomName("Memory1"))
                    {
                        { "setting:///Test6/Member1?prefix=TestPrefix", "Value1" },
                        { "setting:///Member2", "Value2" },
                        { "setting:///Test7/Member", "InvalidValue1" },
                    },
                    new InMemoryProvider(ResourceMetadata.Empty.CustomName("Memory2"))
                    {
                        { "setting:///Test1/Member", "Value1" },
                        { "setting:///Test2/Property", "Value2" },
                        { "setting:///Test4/Property", "Value4" },
                        { "setting:///Test5/Member?prefix=Prefix", "Value5" },
                        { "setting:///Test7/Member", "InvalidValue2" },
                    },
                    new InMemoryProvider(ResourceMetadata.Empty.CustomName("Memory3"))
                    {
                        { "setting:///Test7.Member", "Value7" },
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
                        Where = ImmutableDictionary<string, object>.Empty.Add("_other", nameof(ConfigurationTest))
                    },
                }) //.Select(p => p.DecorateWith(Reusable.SmartConfig.SettingNameProvider.Factory())))
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
                    .AddRow("Test9.Greeting", "Hallo!", nameof(ConfigurationTest));

            using (data)
            {
                SqlHelper.Execute("name=TestDb", connection => { connection.Seed(SettingTableName, data); });
            }
        }

        [Fact]
        public void Can_get_setting_by_name()
        {
            var u = new User
            {
                Configuration = new cfg(new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "User.Name", "Bob" }
                })
            };
            Assert.Equal("Bob", u.Name);
        }

        [Fact]
        public void Can_get_first_setting_by_name()
        {
            var u = new User
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Person.Name", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Bob" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Tom" }
                    }
                }))
            };
            Assert.Equal("Bob", u.Name);
        }

        [Fact]
        public void Can_find_setting_by_provider()
        {
            var u = new Map
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" }, ResourceMetadata.Empty.CustomName("OtherOne"))
                    {
                        { "Map.City", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Map.City", "Tom" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" }, ResourceMetadata.Empty.CustomName("ThisOne"))
                    {
                        { "Map.City", "Bob" }
                    },
                }))
            };
            Assert.Equal("Bob", u.City);
        }

        [Fact]
        public void Can_find_setting_by_provider_type()
        { }

        [Fact]
        public void Can_override_type_and_member_names()
        {
            var u = new Forest
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Forest.Tree", "Tom" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Amazon.Timber", "Bob" }
                    },
                }))
            };
            Assert.Equal("Bob", u.Tree);
        }

        [Fact]
        public void Can_override_setting_name_length()
        {
            var u = new Key
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Reusable.Tests.XUnit.SmartConfig+Key.Location", "Tom" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Door", "Bob" }
                    },
                }))
            };
            Assert.Equal("Tom", u.Location);
            Assert.Equal("Bob", u.Door);
        }

        [Fact]
        public void Can_use_setting_name_prefix()
        {
            var u = new Greeting
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "day:Greeting.Morning", "Bob" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Greeting.Morning", "Tom" }
                    },
                }))
            };
            Assert.Equal("Bob", u.Morning);
        }

        [Fact]
        public void Can_find_setting_on_base_type()
        { }

        [Fact]
        public void Can_deserialize_TimeSpan()
        {
            var settingProviders = new IResourceProvider[]
            {
                new InMemoryProvider(ResourceMetadata.Empty.CustomName("Memory1"))
                {
                    { $"setting:{nameof(CustomTypes)}.{nameof(CustomTypes.TimeSpan)}", "\"00:20:00\"" },
                }
            };
            //.Select(p => p.DecorateWith(Reusable.SmartConfig.SettingNameProvider.Factory()));

            var configuration = new Reusable.SmartConfig.Configuration(new CompositeProvider(settingProviders));
            var customTypes = new CustomTypes { Configuration = configuration };
            Assert.Equal(customTypes.TimeSpan, TimeSpan.FromMinutes(20));
        }
    }

    internal class Nothing
    {
        public IConfiguration Configuration { get; set; }
    }

    // tests defaults
    internal class User : Nothing
    {
        public string Name => Configuration.GetSetting(() => Name);
    }

    // tests annotations
    internal class Admin : Nothing
    {
        //[SettingMember(Name = "Property")]
        [ResourceName("Expertise")]
        public string Skill { get; set; }
    }

    [ResourceName("Amazon")]
    internal class Forest : Nothing
    {
        [ResourceName("Timber")]
        public string Tree => Configuration.GetSetting(() => Tree);
    }

    // tests annotations
    //[SettingType(Prefix = "Prefix")]
    [ResourcePrefix("day")]
    internal class Greeting : Nothing
    {
        public string Morning => Configuration.GetSetting(() => Morning);
    }

    // tests assembly annotations -- they are no longer supported
    // internal class Test6 : Nothing
    // {
    //     public string Member1 { get; set; }
    //
    //     //[SettingMember(Strength = SettingNameStrength.Low, PrefixHandling = PrefixHandling.Disable)]
    //     public string Member2 { get; set; }
    // }

    [ResourceProvider("ThisOne")]
    internal class Map : Nothing
    {
        public string City => Configuration.GetSetting(() => City);
    }

    // provider does not exist
    [ResourceName(Level = ResourceNameLevel.NamespaceTypeMember)]
    internal class Key : Nothing
    {
        public string Location => Configuration.GetSetting(() => Location);
        
        [ResourceName(Level = ResourceNameLevel.Member)]
        public string Door => Configuration.GetSetting(() => Door);
    }

    // tests app-config and sql-server
    internal class Test9 : Nothing
    {
        //[SettingMember(ProviderType = typeof(AppSettingProvider))]
        public string Salute => Configuration.GetSetting(() => Salute);

        //[SettingMember(ProviderType = typeof(SqlServerProvider))]
        public string Greeting
        {
            get => Configuration.GetSetting(() => Greeting);
            set => Configuration.SaveSetting(() => Greeting, value);
        }
    }

    internal class CustomTypes : Nothing
    {
        public TimeSpan TimeSpan => Configuration.GetSetting(() => TimeSpan);
    }
}