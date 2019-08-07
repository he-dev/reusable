using System;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.IOnymous.Config.Annotations;
using Reusable.OneTo1;
using Reusable.Quickey;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

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
namespace Reusable.IOnymous.Config
{
    public class SettingProviderExtensionsTest
    {
        private static readonly ITypeConverter<UriString, string> UriConverter = UriStringQueryToStringConverter.Default;

        [Fact]
        public void test()
        {
            // todo - test ReadSetting request

            var provider = Mock.Create<IResourceProvider>();

            var request = default(Request);
            provider
                .Arrange(x => x.InvokeAsync(Arg.IsAny<Request>()))
                .DoInstead((Request r) => request = r)
                .Returns((Request r) => new PlainResource("John", r.Context).ToTask<IResource>())
                .OccursOnce(); 

            var name = provider.ReadSetting(From<User>.Select(x => x.Name));
        }


        [Fact]
        public void Can_get_setting_by_name()
        {
            var u = new User
            {
                Configuration = new InMemoryProvider(UriConverter)
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
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "Person.Name", "Joe" }
                        })
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "User.Name", "Bob" }
                        })
                        .Add(new InMemoryProvider(UriConverter)
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
                        .Add(new InMemoryProvider(UriConverter, ImmutableContainer.Empty.SetName("OtherOne"))
                        {
                            { "Map.City", "Joe" }
                        })
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "Map.City", "Tom" }
                        })
                        .Add(new InMemoryProvider(UriConverter, ImmutableContainer.Empty.SetName("ThisOne"))
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
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "User.Name", "Joe" }
                        })
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "Forest.Tree", "Tom" }
                        })
                        .Add(new InMemoryProvider(UriConverter)
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
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "User.Name", "Joe" }
                        })
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "Reusable.IOnymous.Config+Key.Location", "Tom" }
                        })
                        .Add(new InMemoryProvider(UriConverter)
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
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "User.Name", "Joe" }
                        })
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "day:Greeting.Morning", "Bob" }
                        })
                        .Add(new InMemoryProvider(UriConverter)
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
                    .Add(new InMemoryProvider(UriConverter)
                    {
                        { "User.Name", "Joe" }
                    })
                    .Add(new InMemoryProvider(UriConverter)
                    {
                        { "User.Name[this]", "Bob" }
                    })
                    .Add(new InMemoryProvider(UriConverter)
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
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "User.Name", "Joe" }
                        })
                        .Add(new InMemoryProvider(UriConverter)
                        {
                            { "Admin.Enabled", true }
                        })
                        .Add(new InMemoryProvider(UriConverter)
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
            var c = CompositeProvider.Empty.Add(new InMemoryProvider(UriConverter)
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
    [PlainSelectorFormatter]
    internal class Nothing
    {
        public IResourceProvider Configuration { get; set; }

        public bool Enabled => Configuration.ReadSetting(() => Enabled);
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
// tests defaults
    internal class User : Nothing
    {
        public string Name => Configuration.ReadSetting(() => Name);
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    internal class Admin : User
    {
        public string Skill => Configuration.ReadSetting(() => Skill);
    }

    [UseType, UseMember]
    [Rename("Amazon")]
    [PlainSelectorFormatter]
    internal class Forest : Nothing
    {
        [Rename("Timber")]
        public string Tree => Configuration.ReadSetting(() => Tree);
    }

    [UseScheme("day"), UseType, UseMember]
    [PlainSelectorFormatter]
    internal class Greeting : Nothing
    {
        public string Morning => Configuration.ReadSetting(() => Morning);
    }

    [UseType, UseMember]
    [Resource(Provider = "ThisOne")]
    [PlainSelectorFormatter]
    internal class Map : Nothing
    {
        public string City => Configuration.ReadSetting(() => City);
    }

    [UseNamespace, UseType, UseMember]
    [PlainSelectorFormatter]
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
        private static readonly ITypeConverter<UriString, string> UriConverter = UriStringQueryToStringConverter.Default;

        [Fact]
        public async Task Can_find_setting_on_base_type()
        {
            var c =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider(UriConverter)
                    {
                        { "User.Name", "Joe" }
                    })
                    .Add(new InMemoryProvider(UriConverter)
                    {
                        { "root:Sub.Enabled", true }
                    })
                    .Add(new InMemoryProvider(UriConverter)
                    {
                        { "Sub.Name", "Tom" }
                    });

            Assert.Equal(true, await c.ReadSettingAsync(From<ISubConfig>.Select(x => x.Enabled)));
            Assert.Equal("Tom", await c.ReadSettingAsync(From<ISubConfig>.Select(x => x.Name)));
        }
    }

    [UseScheme("root"), UseType, UseMember]
    [PlainSelectorFormatter]
    public interface IBaseConfig
    {
        bool Enabled { get; }
    }

    [UseScheme("root"), UseType, UseMember]
    [PlainSelectorFormatter]
    [TrimStart("I"), TrimEnd("Config")]
    public interface ISubConfig : IBaseConfig
    {
        [UseType, UseMember]
        string Name { get; }
    }
}