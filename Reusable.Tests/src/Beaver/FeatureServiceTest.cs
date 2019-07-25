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
        [Fact]
        public void Can_create_key_from_type_and_member()
        {
            Assert.Equal("DemoFeatures.Greeting", DemoFeatures.Greeting);
        }

        [Fact]
        public void Can_configure_features_by_tags()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FeatureOptionRepository>().As<IFeatureOptionRepository>();
            builder.RegisterDecorator<IFeatureOptionRepository>((context, parameters, instance) => new FeatureOptionFallback(instance, FeatureOption.Enable | FeatureOption.Warn | FeatureOption.Telemetry));

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

            features.Options.Batch(names, FeatureOption.Enable, BatchOption.Remove);

            Assert.True(features.Switch(DemoFeatures.Greeting, true, false));
            Assert.True(features.Switch(DemoFeatures.ReadFile, false, true));
            Assert.True(features.Switch(DatabaseFeatures.Commit, false, true));
        }
    }

    namespace Features
    {
        [UseType, UseMember]
        [PlainSelectorFormatter]
        public class DemoFeatures : SelectorBuilder<DemoFeatures>
        {
            public static StringSelector<object> Greeting { get; } = Select(() => Greeting).AsString();

            [Tags("io")]
            public static StringSelector<object> ReadFile { get; } = Select(() => ReadFile).AsString();
        }

        [UseType, UseMember]
        [PlainSelectorFormatter] // todo - comment out to trigger selector-formatter-not-found-exception
        public class DatabaseFeatures : SelectorBuilder<DatabaseFeatures>
        {
            [Tags("io")]
            public static StringSelector<object> Commit { get; } = Select(() => Commit).AsString();
        }
    }
}