using System;
using JetBrains.Annotations;

namespace Reusable.SmartConfig
{
    [PublicAPI]
    public interface IConfiguration
    {
        [CanBeNull]
        object GetValue([NotNull] SoftString settingName, [CanBeNull] Type settingType, [CanBeNull] SoftString dataStoreName);

        void SetValue([NotNull] SoftString settingName, [CanBeNull] object value, [CanBeNull] SoftString dataStoreName);
    }
}