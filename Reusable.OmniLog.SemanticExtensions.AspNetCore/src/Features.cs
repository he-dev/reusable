using System;
using System.Collections.Generic;
using System.Text;
using Reusable.Quickey;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore
{
    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class Features : SelectorBuilder<Features>
    {
        public static Selector<object> LogResponseBody { get; } = Select(() => LogResponseBody);
    }
}
