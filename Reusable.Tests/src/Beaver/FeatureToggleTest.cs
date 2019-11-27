using Xunit;

namespace Reusable.Beaver
{
    public class FeatureToggleTest
    {
        [Fact]
        public void Invokes_main_when_enabled()
        {
            var t = new FeatureToggle().AddOrUpdate(new AlwaysOn("test"));
            var r = t.IIf("test", () => "a", () => "b");

            Assert.Equal("a", r.Value);
            Assert.Equal("Main", r.ToString());
            Assert.Equal(nameof(AlwaysOn), r.Policy.GetType().Name);
            Assert.Equal("test", r.Policy.Feature.Name);
        }

        [Fact]
        public void Invokes_fallback_when_disabled()
        {
            var t = new FeatureToggle().AddOrUpdate(new AlwaysOff("test"));
            var r = t.IIf("test", () => "a", () => "b");

            Assert.Equal("b", r.Value);
            Assert.Equal("Fallback", r.ToString());
            Assert.Equal(nameof(AlwaysOff), r.Policy.GetType().Name);
            Assert.Equal("test", r.Policy.Feature.Name);
        }
    }
}