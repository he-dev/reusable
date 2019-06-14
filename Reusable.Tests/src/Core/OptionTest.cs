using System;
using System.Linq;
using Reusable.Foggle;
using Xunit;

namespace Reusable.Tests.XUnit
{
    public class OptionTest
    {
        [Fact]
        public void Examples()
        {
            Assert.Equal(new[] { 0, 1, 2, 4 }, new[]
            {
                FeatureOption.None,
                FeatureOption.Enable,
                FeatureOption.Warn,
                FeatureOption.Telemetry
            }.Select(o => o.Flag));

            Assert.Equal(FeatureOption.Enable, FeatureOption.Enable);
            Assert.NotEqual(FeatureOption.Enable, FeatureOption.Telemetry);

            var fromName = FeatureOption.FromName("Warn");
            Assert.Equal(FeatureOption.Warn, fromName);

            var fromValue = FeatureOption.FromValue(3);
            var enableWarn = FeatureOption.Enable | FeatureOption.Warn;
            Assert.Equal(enableWarn, fromValue);

            var names = $"{enableWarn:names}";
            var flags = $"{enableWarn:flags}";
            var namesAndFlags = $"{enableWarn:names+flags}";
            var @default = $"{enableWarn}";

            Assert.True(FeatureOption.None < FeatureOption.Enable);
            Assert.True(FeatureOption.Enable < FeatureOption.Telemetry);

            Assert.Throws<ArgumentOutOfRangeException>(() => FeatureOption.FromValue(1000));
            //Assert.ThrowsAny<DynamicException>(() => FeatureOption.Create("All", 111111));
        }
    }
}