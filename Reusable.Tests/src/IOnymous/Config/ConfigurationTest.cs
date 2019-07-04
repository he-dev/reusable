using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.IOnymous.Config;
using Reusable.IOnymous.Config.Annotations;
using Reusable.Quickey;
using Xunit;

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
namespace Reusable.Tests.IOnymous.Config
{
    using static SettingProvider;

    public class ConfigurationTest
    {
        [Fact]
        public void Can_get_setting_by_name()
        {
            var u = new User
            {
                Configuration = new InMemoryProvider(DefaultUriStringConverter)
                {
                    { "User.Name", "Bob" }
                }
            };
            Assert.Equal("Bob", u.Name);
        }

        [Fact]
        public void Can_get_first_setting_by_name()
        {
            var u = new User
            {
                Configuration =
                    CompositeProvider
                        .Empty
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "Person.Name", "Joe" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "User.Name", "Bob" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "User.Name", "Tom" }
                        })
            };

            Assert.Equal("Bob", u.Name);
        }

        [Fact]
        public void Can_find_setting_by_provider()
        {
            var u = new Map
            {
                Configuration =
                    CompositeProvider
                        .Empty
                        .Add(new InMemoryProvider(DefaultUriStringConverter, ImmutableSession.Empty.SetName("OtherOne"))
                        {
                            { "Map.City", "Joe" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "Map.City", "Tom" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter, ImmutableSession.Empty.SetName("ThisOne"))
                        {
                            { "Map.City", "Bob" }
                        })
            };

            Assert.Equal("Bob", u.City);
        }

        [Fact]
        public void Can_find_setting_by_provider_type() { }

        [Fact]
        public void Can_override_type_and_member_names()
        {
            var u = new Forest
            {
                Configuration =
                    CompositeProvider
                        .Empty
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "User.Name", "Joe" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "Forest.Tree", "Tom" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "Amazon.Timber", "Bob" }
                        })
            };

            Assert.Equal("Bob", u.Tree);
        }

        [Fact]
        public void Can_override_setting_name_length()
        {
            var u = new Key
            {
                Configuration =
                    CompositeProvider
                        .Empty
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "User.Name", "Joe" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "Reusable.Tests.IOnymous.Config+Key.Location", "Tom" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "Door", "Bob" }
                        })
            };

            Assert.Equal("Tom", u.Location);
            Assert.Equal("Bob", u.Door);
        }

        [Fact]
        public void Can_use_setting_name_prefix()
        {
            var u = new Greeting
            {
                Configuration =
                    CompositeProvider
                        .Empty
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "User.Name", "Joe" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "day:Greeting.Morning", "Bob" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "Greeting.Morning", "Tom" }
                        })
            };

            Assert.Equal("Bob", u.Morning);
        }

        [Fact]
        public async Task Can_use_setting_with_handle()
        {
            var c =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider(DefaultUriStringConverter)
                    {
                        { "User.Name", "Joe" }
                    })
                    .Add(new InMemoryProvider(DefaultUriStringConverter)
                    {
                        { "User.Name[this]", "Bob" }
                    })
                    .Add(new InMemoryProvider(DefaultUriStringConverter)
                    {
                        { "Greeting.Morning", "Tom" }
                    });

            Assert.Equal("Bob", await c.ReadSettingAsync(From<User>.Select(x => x.Name).Index("this")));
        }

        [Fact]
        public void Can_find_setting_on_base_type()
        {
            var u = new Admin
            {
                Configuration =
                    CompositeProvider
                        .Empty
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "User.Name", "Joe" }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "Admin.Enabled", true }
                        })
                        .Add(new InMemoryProvider(DefaultUriStringConverter)
                        {
                            { "Admin.Skill", "Tom" }
                        })
            };

            Assert.Equal(true, u.Enabled);
            Assert.Equal("Tom", u.Skill);
        }

        [Fact]
        public async Task Can_save_setting()
        {
            var c = CompositeProvider.Empty.Add(new InMemoryProvider(DefaultUriStringConverter)
            {
                { "Admin.Name", "Joe" },
                { "Admin.Enabled", true }
            });

            var selectAdminName = From<Admin>.Select(x => x.Name);
            var selectAdminEnabled = From<Admin>.Select(x => x.Enabled);

            Assert.Equal("Joe", await c.ReadSettingAsync(selectAdminName));
            Assert.Equal(true, await c.ReadSettingAsync(selectAdminEnabled));
            await c.WriteSettingAsync(selectAdminName, "Tom");
            await c.WriteSettingAsync(selectAdminEnabled, false);
            Assert.Equal("Tom", await c.ReadSettingAsync(selectAdminName));
            Assert.Equal(false, await c.ReadSettingAsync(selectAdminEnabled));
        }
    }

    [UseType, UseMember]
    [SettingSelectorFormatter]
    internal class Nothing
    {
        public IResourceProvider Configuration { get; set; }

        public bool Enabled => Configuration.ReadSetting(() => Enabled);
    }

    [UseType, UseMember]
    [SettingSelectorFormatter]
// tests defaults
    internal class User : Nothing, INamespace
    {
        public string Name => Configuration.ReadSetting(() => Name);
    }

    [UseType, UseMember]
    [SettingSelectorFormatter]
    internal class Admin : User
    {
        public string Skill => Configuration.ReadSetting(() => Skill);
    }

    [UseType, UseMember]
    [Rename("Amazon")]
    [SettingSelectorFormatter]
    internal class Forest : Nothing
    {
        [Rename("Timber")]
        public string Tree => Configuration.ReadSetting(() => Tree);
    }

    [UseScheme("day"), UseType, UseMember]
    [SettingSelectorFormatter]
    internal class Greeting : Nothing
    {
        public string Morning => Configuration.ReadSetting(() => Morning);
    }

// tests assembly annotations -- they are no longer supported
// internal class Test6 : Nothing
// {
//     public string Member1 { get; set; }
//
//     //[SettingMember(Strength = SettingNameStrength.Low, PrefixHandling = PrefixHandling.Disable)]
//     public string Member2 { get; set; }
// }

    [UseType, UseMember]
    [Resource(Provider = "ThisOne")]
    [SettingSelectorFormatter]
    internal class Map : Nothing
    {
        public string City => Configuration.ReadSetting(() => City);
    }

    [UseNamespace, UseType, UseMember]
    [SettingSelectorFormatter]
    internal class Key : Nothing
    {
        public string Location => Configuration.ReadSetting(() => Location);

        [UseMember]
        public string Door => Configuration.ReadSetting(() => Door);
    }

    internal class CustomTypes : Nothing
    {
        public TimeSpan TimeSpan => Configuration.ReadSetting(() => TimeSpan);
    }

    public class ConfigurationTestGeneric
    {
        [Fact]
        public async Task Can_find_setting_on_base_type()
        {
            var c =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider(DefaultUriStringConverter)
                    {
                        { "User.Name", "Joe" }
                    })
                    .Add(new InMemoryProvider(DefaultUriStringConverter)
                    {
                        { "root:Sub.Enabled", true }
                    })
                    .Add(new InMemoryProvider(DefaultUriStringConverter)
                    {
                        { "Sub.Name", "Tom" }
                    });

            Assert.Equal(true, await c.ReadSettingAsync(From<ISubConfig>.Select(x => x.Enabled)));
            Assert.Equal("Tom", await c.ReadSettingAsync(From<ISubConfig>.Select(x => x.Name)));
        }
    }

    [UseScheme("root"), UseType, UseMember]
    [SettingSelectorFormatter]
    public interface IBaseConfig : INamespace
    {
        bool Enabled { get; }
    }

    [UseScheme("root"), UseType, UseMember]
    [SettingSelectorFormatter]
    [TrimStart("I"), TrimEnd("Config")]
    public interface ISubConfig : IBaseConfig
    {
        [UseType, UseMember]
        string Name { get; }
    }

    
}