using System;
using System.Collections.Immutable;
using Reusable.Beaver;
using Reusable.Data;
using Xunit;

namespace Reusable.Tests.Data
{
    public class OptionTest
    {
        [Fact]
        public void Examples2()
        {
//            Assert.Equal(new[] { 0, 1, 2, 4 }, new[]
//            {
//                FeatureOption.None,
//                FeatureOption.Enable,
//                FeatureOption.Warn,
//                FeatureOption.Telemetry
//            }.Select(o => o.Flag));

            Assert.Equal(FeatureOption.Enabled, FeatureOption.Enabled);
            Assert.NotEqual(FeatureOption.Enabled, FeatureOption.Telemetry);

            var fromName = FeatureOption.FromName("Warn");
            Assert.Equal(FeatureOption.WarnIfDirty, fromName);

            //var fromValue = FeatureOption.FromValue(3);
            var enableWarn = FeatureOption.Enabled | FeatureOption.WarnIfDirty;
            //Assert.Equal(enableWarn, fromValue);

            var names = $"{enableWarn:names}";
            var flags = $"{enableWarn:flags}";
            var namesAndFlags = $"{enableWarn:names+flags}";
            var @default = $"{enableWarn}";

            //Assert.True(FeatureOption.None < FeatureOption.Enable);
            //Assert.True(FeatureOption.Enable < FeatureOption.Telemetry);

            //Assert.Throws<ArgumentOutOfRangeException>(() => FeatureOption.FromValue(1000));
            //Assert.ThrowsAny<DynamicException>(() => FeatureOption.Create("All", 111111));
        }

        [Fact]
        public void Can_compare_options_for_equality()
        {
            Assert.Equal(TestOption.Two, TestOption.Two);
            Assert.NotEqual(TestOption.Two, TestOption.Three);
        }

        [Fact]
        public void Can_set_flags()
        {
            var actual = TestOption.One | TestOption.Two | TestOption.Three;
            Assert.True(actual.Contains(TestOption.One));
            Assert.True(actual.Contains(TestOption.Two));
            Assert.True(actual.Contains(TestOption.Three));
        }
        
        [Fact]
        public void Can_reset_flags()
        {
            var actual = TestOption.One.RemoveFlag(TestOption.One);
            Assert.False(actual.Contains(TestOption.One));
            Assert.Equal(TestOption.None, actual);
        }

        private class TestOption : Option<TestOption>
        {
            public TestOption(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

            public static readonly TestOption One = CreateWithCallerName();
            public static readonly TestOption Two = CreateWithCallerName();
            public static readonly TestOption Three = CreateWithCallerName();
            public static readonly TestOption Custom = Create("Other");
        }
    }
}