using System.Collections.Immutable;
using Reusable.Data;

namespace Reusable.Beaver
{
    public abstract class Feature
    {
        public static class Options
        {
            /// <summary>
            /// Indicates that a feature is enabled.
            /// </summary>
            public static readonly Option<Feature> Enabled = Option<Feature>.CreateWithCallerName();

            /// <summary>
            /// Indicates that a warning should be logged when a feature is dirty.
            /// </summary>
            public static readonly Option<Feature> WarnIfDirty = Option<Feature>.CreateWithCallerName();

            /// <summary>
            /// Indicates that feature telemetry should be logged.
            /// </summary>
            public static readonly Option<Feature> Telemetry = Option<Feature>.CreateWithCallerName();

            /// <summary>
            /// Indicates that a feature should be toggled after each execution.
            /// </summary>
            public static readonly Option<Feature> Toggle = Option<Feature>.CreateWithCallerName();

            /// <summary>
            /// Indicates that a feature should be toggled only once.
            /// </summary>
            public static readonly Option<Feature> ToggleOnce = Option<Feature>.CreateWithCallerName();

            /// <summary>
            /// Indicates that feature-options should be reset.
            /// </summary>
            public static readonly Option<Feature> ToggleReset = Option<Feature>.CreateWithCallerName();

            /// <summary>
            /// Indicates that feature-options must not be changed.
            /// </summary>
            public static readonly Option<Feature> Locked = Option<Feature>.CreateWithCallerName();
        }
    }
}