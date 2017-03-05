using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Logging.NLog.Tools.LayoutRenderers
{
    [LayoutRenderer("invariant-properties")]
    public class InvariantPropertiesLayoutRenderer : LayoutRenderer
    {
        public string Item { get; set; }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(Item)) throw new InvalidOperationException("You need to specify the 'Item' that is a log-event property.");
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", logEvent.Properties[Item]);
        }

        public static void Register()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("invariant-properties", typeof(InvariantPropertiesLayoutRenderer));
        }
    }
}
