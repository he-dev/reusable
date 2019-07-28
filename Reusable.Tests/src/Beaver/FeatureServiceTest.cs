using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Autofac;
using Reusable.Beaver;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.Quickey;
using Reusable.Tests.Beaver.Features;
using Xunit;

namespace Reusable.Tests.Beaver
{
    public class FeatureServiceTest
    {
//        [Fact]
//        public void Can_create_key_from_type_and_member()
//        {
//            Assert.Equal("DemoFeatures.Greeting", DemoFeatures.Greeting);
//        }

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
        public void Can_undo_feature_toggle()
        {
            var featureToggle = new FeatureToggle(new FeatureOptionRepository()).DecorateWith<IFeatureToggle>(instance => new FeatureToggler(instance));
            Assert.False(featureToggle.IsEnabled("test")); // it disabled by default
            featureToggle.EnableToggler("test"); // activate feature-toggler
            Assert.True(featureToggle.Switch("test", false, true)); // it's still disabled and will now switch
            Assert.True(featureToggle.IsEnabled("test")); // now it should be enabled
            Assert.True(featureToggle.Switch("test", true, false)); 
            Assert.True(featureToggle.Switch("test", true, false)); 
            Assert.True(featureToggle.IsEnabled("test")); // now it should be still be enabled because it was a one-time-switch
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