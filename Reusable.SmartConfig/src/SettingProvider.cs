using Reusable.IOnymous;

namespace Reusable.SmartConfig
{
    public abstract class SettingProvider : ResourceProvider
    {
        protected SettingProvider(Metadata metadata)
            : base(new SoftString[] { "setting" }, metadata) { }
    }
}