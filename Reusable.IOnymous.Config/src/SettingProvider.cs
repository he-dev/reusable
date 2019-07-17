using System.Collections.Immutable;
using Reusable.Data;
using Reusable.OneTo1;

namespace Reusable.IOnymous.Config
{
    public abstract class SettingProvider : ResourceProvider
    {
        protected SettingProvider(IImmutableContainer properties)
            : base(properties.SetScheme("config")) { }
    }
}