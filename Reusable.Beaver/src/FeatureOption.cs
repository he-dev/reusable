using System.Collections.Immutable;
using Reusable.Data;

namespace Reusable.Beaver
{
    public class FeatureOption : Option<FeatureOption>
    {
        public FeatureOption(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        /// <summary>
        /// Indicates that a feature is enabled.
        /// </summary>
        public static readonly FeatureOption Enabled = CreateWithCallerName();

        /// <summary>
        /// Indicates that a warning should be logged when a feature is dirty.
        /// </summary>
        public static readonly FeatureOption WarnIfDirty = CreateWithCallerName();

        /// <summary>
        /// Indicates that feature telemetry should be logged.
        /// </summary>
        public static readonly FeatureOption Telemetry = CreateWithCallerName();
        
        /// <summary>
        /// Indicates that a feature should be toggled after each execution.
        /// </summary>
        public static readonly FeatureOption Toggle = CreateWithCallerName();
        
        /// <summary>
        /// Indicates that a feature should be toggled only once.
        /// </summary>
        public static readonly FeatureOption ToggleOnce = CreateWithCallerName();
        
        /// <summary>
        /// Indicates that feature-options should be reset.
        /// </summary>
        public static readonly FeatureOption ToggleReset = CreateWithCallerName();
        
        /// <summary>
        /// Indicates that feature-options must not be changed.
        /// </summary>
        public static readonly FeatureOption Locked = CreateWithCallerName();
        
//        /// <summary>
//        /// Indicates that feature-options have been changed.
//        /// </summary>
//        public static readonly FeatureOption Dirty = CreateWithCallerName();
        
//        // You use this to distinguish between FeatureOption.None which results in default-options.
//        /// <summary>
//        /// Indicates that feature-options have been saved.
//        /// </summary>
//        public static readonly FeatureOption Saved = CreateWithCallerName();
        
    }
}