using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public abstract class SettingProvider : ResourceProvider
    {
        protected SettingProvider(Metadata metadata)
            : base(new SoftString[] { "setting" }, metadata)
        {
        }
    }
}