using Reusable.Data;
using Reusable.IOnymous;

namespace Reusable.SmartConfig
{
    public abstract class SettingProvider : ResourceProvider
    {
        protected SettingProvider(IImmutableSession metadata)
            : base(Configuration.DefaultSchemes, metadata) { }
    }
}