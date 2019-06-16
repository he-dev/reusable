using System.Collections.Immutable;

namespace Reusable.Beaver
{
    public class FeatureOption : Option<FeatureOption>
    {
        public FeatureOption(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        /// <summary>
        /// When set a feature is enabled.
        /// </summary>
        public static readonly FeatureOption Enable = CreateWithCallerName();

        /// <summary>
        /// When set a warning is logged when a feature is toggled.
        /// </summary>
        public static readonly FeatureOption Warn = CreateWithCallerName();

        /// <summary>
        /// When set feature usage statistics are logged.
        /// </summary>
        public static readonly FeatureOption Telemetry = CreateWithCallerName();

        public static readonly FeatureOption Default = CreateWithCallerName(Enable | Warn);
    }
}