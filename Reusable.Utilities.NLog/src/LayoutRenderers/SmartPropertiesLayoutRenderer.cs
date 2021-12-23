using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using Reusable.Essentials;

namespace Reusable.Utilities.NLog.LayoutRenderers
{
    /// <summary>
    /// This layout-renderer provides a case-insensitive access to logger properties and formats the value with an invariant culture by default.
    /// </summary>
    [UsedImplicitly, PublicAPI]
    [LayoutRenderer(Name)]
    public class SmartPropertiesLayoutRenderer : LayoutRenderer
    {
        // Makes subsequent lookups a little bit faster as the entire dictionary does not need to be scanned again.
        private object InnerKey { get; set; }

        public const string Name = "smart-properties";

        public string Key { get; set; }

        public string Culture { get; set; } = string.Empty;

        public string Format { get; set; }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(Key)) throw new InvalidOperationException("You need to specify the property key.");
            
            InnerKey ??= logEvent.Properties.SingleOrDefault(p => p.Key is string s && SoftString.Comparer.Equals(Key, s)).Key;

            if (InnerKey is {} && logEvent.Properties.TryGetValue(InnerKey, out var value) && value is {})
            {
                builder.AppendFormat(new CultureInfo(Culture), $"{{0{(Format is null ? string.Empty : ":" + Format)}}}", value);
            }

            // var property = logEvent.Properties.SingleOrDefault(p => p.Key is string s && Comparer.Equals(Key, s));
            // if (!(property.Value is null))
            // {
            //     builder.AppendFormat(new CultureInfo(Culture), $"{{0{(Format is null ? string.Empty : ":" + Format)}}}", property.Value);
            // }
        }

        /// <summary>
        /// This helper method allows to easily register this renderer.
        /// </summary>
        public static void Register()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition(Name, typeof(SmartPropertiesLayoutRenderer));
        }
    }
}