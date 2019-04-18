using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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

        public ConfigurationTest()
        {
            // new AppSettingProvider(new UriStringToSettingIdentifierConverter()),
            // new SqlServerProvider("name=TestDb", new UriStringToSettingIdentifierConverter())
            // {
            //     TableName = SettingTableName,
            //     ColumnMappings =
            //         ImmutableDictionary<SqlServerColumn, SoftString>
            //             .Empty
            //             .Add(SqlServerColumn.Name, "_name")
            //             .Add(SqlServerColumn.Value, "_value"),
            //     Where = ImmutableDictionary<string, object>.Empty.Add("_other", nameof(ConfigurationTest))
            // },
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
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" }, Metadata.Empty.CustomName("OtherOne"))
                    {
                        { "Map.City", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Map.City", "Tom" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" }, Metadata.Empty.CustomName("ThisOne"))
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
        {
            var u = new Admin
            {
                Configuration = new cfg(new CompositeProvider(new IResourceProvider[]
                {
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "User.Name", "Joe" }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Admin.Enabled", true }
                    },
                    new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                    {
                        { "Admin.Skill", "Tom" }
                    },
                }))
            };
            Assert.Equal(true, u.Enabled);
            Assert.Equal("Tom", u.Skill);
        }

        [Fact]
        public void Can_deserialize_TimeSpan()
        {
            var settingProviders = new IResourceProvider[]
            {
                new InMemoryProvider(Metadata.Empty.CustomName("Memory1"))
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

        public bool Enabled => Configuration.GetItem(() => Enabled);
    }

    // tests defaults
    internal class User : Nothing
    {
        public string Name => Configuration.GetItem(() => Name);
    }

    internal class Admin : Nothing
    {
        public string Skill => Configuration.GetItem(() => Skill);
    }

    [ResourceName("Amazon")]
    internal class Forest : Nothing
    {
        [ResourceName("Timber")]
        public string Tree => Configuration.GetItem(() => Tree);
    }

    [ResourcePrefix("day")]
    internal class Greeting : Nothing
    {
        public string Morning => Configuration.GetItem(() => Morning);
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
        public string City => Configuration.GetItem(() => City);
    }

    [ResourceName(Level = ResourceNameLevel.NamespaceTypeMember)]
    internal class Key : Nothing
    {
        public string Location => Configuration.GetItem(() => Location);

        [ResourceName(Level = ResourceNameLevel.Member)]
        public string Door => Configuration.GetItem(() => Door);
    }    

    internal class CustomTypes : Nothing
    {
        public TimeSpan TimeSpan => Configuration.GetItem(() => TimeSpan);
    }
    
    public class ConfigurationTestGeneric
    {
        [Fact]
        public async Task Can_find_setting_on_base_type()
        {
            var c = new Reusable.SmartConfig.Configuration<ISubConfig>(new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "User.Name", "Joe" }
                },
                new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "root:Sub.Enabled", true }
                },
                new InMemoryProvider(new UriStringToSettingIdentifierConverter(), new SoftString[] { "setting" })
                {
                    { "Sub.Name", "Tom" }
                },
            }));
            
            Assert.Equal(true, await c.GetItemAsync(x => x.Enabled));
            Assert.Equal("Tom", await c.GetItemAsync(x => x.Name));
        }
    }

    [ResourcePrefix("root")]
    public interface IBaseConfig
    {
        bool Enabled { get; }
    }

    public interface ISubConfig : IBaseConfig
    {
        [ResourcePrefix("")]
        string Name { get; }
    }
}