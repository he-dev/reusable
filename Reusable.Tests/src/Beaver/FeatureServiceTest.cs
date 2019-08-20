using System.Collections.Immutable;
using Autofac;
using Reusable.Beaver.Features;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.Quickey;
using Xunit;

namespace Reusable.Beaver
{
    public class FeatureServiceTest
    {
        [Fact]
        public void Can_configure_features_by_tags()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FeatureOptionRepository>().As<IFeatureOptionRepository>();
            builder.RegisterDecorator<IFeatureOptionRepository>((context, parameters, instance) => new FeatureOptionFallback.Enabled(instance));

            var container = builder.Build();
            var options = container.Resolve<IFeatureOptionRepository>();

            var features = new FeatureToggle(options);

            var names =
                ImmutableList<Selector>
                    .Empty
                    .AddFrom<DemoFeatures>()
                    .AddFrom<DatabaseFeatures>()
                    .Where<TagsAttribute>("io")
                    .Format();

            features.Options.Batch(names, FeatureOption.Enabled, BatchOption.Remove);
            features.Options.SaveChanges();

            Assert.True(features.Switch(DemoFeatures.Greeting, true, false));
            Assert.True(features.Switch(DemoFeatures.ReadFile, false, true));
            Assert.True(features.Switch(DatabaseFeatures.Commit, false, true));
        }

        [Fact]
        public void Can_toggle_feature()
        {
            var features = new FeatureToggle(new FeatureOptionRepository()).DecorateWith<IFeatureToggle>(instance => new FeatureToggler(instance));
            Assert.False(features.IsEnabled("test")); // it disabled by default
            features.Update("test", f => f.Set(FeatureOption.Toggle).Set(FeatureOption.ToggleOnce)); // activate feature-toggler
            Assert.True(features.Switch("test", false, true)); // it's still disabled and will now switch
            Assert.True(features.IsEnabled("test")); // now it should be enabled
            Assert.True(features.Switch("test", true, false)); 
            Assert.True(features.Switch("test", true, false)); 
            Assert.True(features.IsEnabled("test")); // now it should be still be enabled because it was a one-time-switch
        }
    }

    namespace Features
    {
        [UseType, UseMember]
        [PlainSelectorFormatter]
        public class DemoFeatures : SelectorBuilder<DemoFeatures>
        {
            public static Selector<object> Greeting { get; } = Select(() => Greeting);

            [Tags("io")]
            public static Selector<object> ReadFile { get; } = Select(() => ReadFile);
        }

        [UseType, UseMember]
        [PlainSelectorFormatter] // todo - comment out to trigger selector-formatter-not-found-exception
        public class DatabaseFeatures : SelectorBuilder<DatabaseFeatures>
        {
            [Tags("io")]
            public static Selector<object> Commit { get; } = Select(() => Commit);
        }
    }
}