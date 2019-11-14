using Reusable.Quickey;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore
{
    [UseType, UseMember]
    [PlainSelectorFormatter]
    public abstract class Features : SelectorBuilder<Features>
    {
        public static Selector<object> LogResponseBody { get; } = Select(() => LogResponseBody);
    }
}
