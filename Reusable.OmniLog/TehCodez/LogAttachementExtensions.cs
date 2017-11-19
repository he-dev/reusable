using System.Collections.Generic;

namespace Reusable.OmniLog
{
    public static class LogAttachementExtensions
    {
        public static KeyValuePair<SoftString, object> ToLogProperty(this ILogAttachement property)
        {
            return new KeyValuePair<SoftString, object>(property.Name, property);
        }
    }
}