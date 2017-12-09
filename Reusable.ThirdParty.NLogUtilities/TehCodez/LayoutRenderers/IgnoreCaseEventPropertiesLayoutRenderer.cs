using System;
using System.Collections.Generic;
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

        //private static readonly IEqualityComparer<object> Comparer;

        //static IgnoreCaseEventPropertiesLayoutRenderer()
        //{
        //    Comparer = RelayEqualityComparer<object>.Create(
        //        (left, right) =>
        //        {
        //            return
        //                (left is string s1 && right is string s2)
        //                    ? StringComparer.OrdinalIgnoreCase.Equals(s1, s2)
        //                    : left.Equals(right);
        //        },
        //        (obj) =>
        //        {
        //            return
        //                (obj is string s)
        //                    ? StringComparer.OrdinalIgnoreCase.GetHashCode(s)
        //                    : obj.GetHashCode();
        //        }
        //    );
        //}

        public string Item { get; set; }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (string.IsNullOrEmpty(Item)) throw new InvalidOperationException("You need to specify the 'Item' that is a log-event property.");

            var property = logEvent.Properties.SingleOrDefault(p => p.Key is string s && StringComparer.OrdinalIgnoreCase.Equals(Item, s));
            if (!(property.Value is null))
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", property.Value);
            }
        }

        public static void Register()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition(Name, typeof(IgnoreCaseEventPropertiesLayoutRenderer));
        }
    }
}