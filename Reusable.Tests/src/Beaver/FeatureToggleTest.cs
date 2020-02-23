using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Reusable.Beaver.Policies;
using Reusable.Translucent;
using Reusable.Utilities.JsonNet;
using Xunit;

namespace Reusable.Beaver
{
    public class FeatureToggleTest
    {
        [Fact]
        public async Task Uses_primary_action_when_enabled_otherwise_secondary()
        {
            var t = new FeatureController(FeaturePolicy.AlwaysOff)
            {
                { "a", FeaturePolicy.AlwaysOn },
                { "b", FeaturePolicy.AlwaysOff },
            };

            // async
            Assert.Equal("primary", await t.Use("a", () => Task.FromResult("primary"), () => Task.FromResult("secondary")));
            Assert.Equal("secondary", await t.Use("b", () => Task.FromResult("primary"), () => Task.FromResult("secondary")));
            Assert.Equal("primary", await t.Use("a", () => Task.FromResult("primary")));
            Assert.Equal(default(string), await t.Use("b", () => Task.FromResult("primary")));

            // non-async
            Assert.Equal("primary", t.Use("a", () => "primary", () => "secondary"));
            Assert.Equal("secondary", t.Use("b", () => "primary", () => "secondary"));
            Assert.Equal("primary", t.Use("a", () => "primary"));
            Assert.Equal(default(string), t.Use("b", () => "primary"));

            // value
            Assert.Equal("primary", t.Use("a", "primary", "secondary"));
            Assert.Equal("secondary", t.Use("b", "primary", "secondary"));
            Assert.Equal("primary", t.Use("a", "primary"));
            Assert.Equal(default(string), t.Use("b", "primary"));
        }

        [Fact]
        public void Uses_secondary_action_when_feature_not_found()
        {
            var t = new FeatureController(FeaturePolicy.AlwaysOff) { { "a", FeaturePolicy.AlwaysOn } };

            var a = t.Use("a", "x", "y");
            var b = t.Use("b", "x", "y");

            Assert.Equal("x", a);
            Assert.Equal("y", b);

            Assert.Equal(FeatureState.Enabled, a.State);
            Assert.Equal(FeatureState.Disabled, b.State);

            Assert.IsType<Feature>(a.Feature);
            Assert.IsType<AlwaysOn>(a.Feature.Policy);

            Assert.IsType<Feature.Fallback>(b.Feature);
            Assert.IsType<Lock>(b.Feature.Policy);
        }

        [Fact]
        public void Once_disables_feature_after_first_use()
        {
            var t = new FeatureController(FeaturePolicy.AlwaysOff) { { "a", FeaturePolicy.Once } };

            var r = new[]
            {
                t.Use("a", "x", "y"),
                t.Use("a", "x", "y"),
                t.Use("a", "x", "y"),
            };

            Assert.Collection
            (
                r,
                x => { Assert.Equal("x", x); },
                x => { Assert.Equal("y", x); },
                x => { Assert.Equal("y", x); }
            );
        }

        [Fact]
        public void Lambda_uses_Func()
        {
            var q = 0;
            var t = new FeatureController(new FeatureToggle(FeaturePolicy.AlwaysOff));
            t.Add("test", FeaturePolicy.Ask(_ => q++ < 1));

            var m = 0;
            var f = 0;
            var a = t.Use("test", () => ++m, () => ++f);
            var b = t.Use("test", () => ++m, () => ++f);
            Assert.Equal(2, q);
            Assert.Equal(1, m);
            Assert.Equal(1, f);
            Assert.Equal(1, a);
            Assert.Equal(1, b);
            Assert.Equal(FeatureState.Enabled, a.State);
            Assert.Equal(FeatureState.Disabled, b.State);
            Assert.IsType<Lambda>(a.Feature.Policy);
            Assert.IsType<Lambda>(b.Feature.Policy);
            Assert.Equal("test", a.Feature.Name);
            Assert.Equal("test", b.Feature.Name);
        }

        [Fact]
        public void Throws_when_modifying_locked_feature()
        {
            var t = new FeatureToggle(FeaturePolicy.AlwaysOff);
            t.Add("test", FeaturePolicy.AlwaysOn.Lock());
            //t.SetOrUpdate("test", FeaturePolicy.AlwaysOff);
            Assert.Throws<InvalidOperationException>(() => t["test"].Policy = FeaturePolicy.AlwaysOff);
        }
    }
}