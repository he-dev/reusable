using System;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace Reusable.ThirdParty.NLogUtilities.LayoutRenderers
{
    [UsedImplicitly, PublicAPI]
    [LayoutRenderer(Name)]
    public class IgnoreCaseEventPropertiesLayoutRenderer : LayoutRenderer
    {
        public const string Name = "ignore-case-event-properties";

        public string Item { get; set; }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(Item)) throw new InvalidOperationException("You need to specify the 'Item' that is a log-event property.");
            var key = logEvent.Properties.Keys.OfType<string>().SingleOrDefault(k => k.Equals(Item, StringComparison.OrdinalIgnoreCase));
            if (!(key is null))
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", logEvent.Properties[key]);
            }
        }

        public static void Register()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition(Name, typeof(IgnoreCaseEventPropertiesLayoutRenderer));
        }
    }
}