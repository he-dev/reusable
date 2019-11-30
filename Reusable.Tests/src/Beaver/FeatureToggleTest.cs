using System;
using Reusable.Beaver.Policies;
using Xunit;

namespace Reusable.Beaver
{
    public class FeatureToggleTest
    {
        [Fact]
        public void Invokes_main_when_enabled()
        {
            var t = new FeatureToggle().AddOrUpdate(new AlwaysOn("test"));

            var a = 0;
            var b = 0;
            
            var c = t.IIf("test", () => ++a, () => ++b);
            var d = t.IIf("test", () => ++a, () => ++b);

            Assert.Equal(2, a);
            Assert.Equal(0, b);
            Assert.Equal(1, c);
            Assert.Equal(2, d);
            
            Assert.IsType<FeatureActionResult<int>.Main>(c);
            Assert.IsType<AlwaysOn>(c.Policy);
            Assert.Equal("test", c.Policy.Feature.Name);
        }

        [Fact]
        public void Invokes_fallback_when_disabled()
        {
            var t = new FeatureToggle().AddOrUpdate(new AlwaysOff("test"));
            
            var a = 0;
            var b = 0;
            
            var c = t.IIf("test", () => ++a, () => ++b);
            var d = t.IIf("test", () => ++a, () => ++b);

            Assert.Equal(0, a);
            Assert.Equal(2, b);
            Assert.Equal(1, c);
            Assert.Equal(2, d);

            Assert.IsType<FeatureActionResult<int>.Fallback>(c);
            Assert.IsType<AlwaysOff>(c.Policy);
            Assert.Equal("test", c.Policy.Feature.Name);
        }
        
        [Fact]
        public void Once_disables_itself_after_first_invoke()
        {
            var t = new FeatureToggle().AddOrUpdate(new Once("test"));
            
            var a = 0;
            var b = 0;
            
            var c = t.IIf("test", () => ++a, () => ++b);
            var d = t.IIf("test", () => ++a, () => ++b);

            Assert.Equal(1, a);
            Assert.Equal(1, b);
            Assert.Equal(1, c);
            Assert.Equal(1, d);

            Assert.IsType<FeatureActionResult<int>.Main>(c);
            Assert.IsType<FeatureActionResult<int>.Fallback>(d);
            Assert.IsType<Once>(c.Policy);
            Assert.IsType<AlwaysOff>(d.Policy);
            Assert.Equal("test", c.Policy.Feature.Name);
        }
        
        [Fact]
        public void Ask_requests_permission_to_invoke()
        {
            var q = 0;
            var a = 0;
            var b = 0;
            
            var t = new FeatureToggle().AddOrUpdate(new Ask("test", f => q++ < 1));
            
            var c = t.IIf("test", () => ++a, () => ++b);
            var d = t.IIf("test", () => ++a, () => ++b);

            Assert.Equal(2, q);
            Assert.Equal(1, a);
            Assert.Equal(1, b);
            Assert.Equal(1, c);
            Assert.Equal(1, d);

            Assert.IsType<FeatureActionResult<int>.Main>(c);
            Assert.IsType<FeatureActionResult<int>.Fallback>(d);
            Assert.IsType<Ask>(c.Policy);
            Assert.Equal("test", c.Policy.Feature.Name);
        }

        [Fact]
        public void Throws_when_modifying_locked_feature()
        {
            var t = new FeatureToggle().AddOrUpdate(new AlwaysOn("test").Lock());

            Assert.Throws<InvalidOperationException>(() => t.AddOrUpdate(new AlwaysOff("test")));
        }
    }
}