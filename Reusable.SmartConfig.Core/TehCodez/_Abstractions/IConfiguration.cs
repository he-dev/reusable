using System;
using JetBrains.Annotations;

namespace Reusable.SmartConfig
{
    [PublicAPI]
    public interface IConfiguration
    {
        [CanBeNull]
        object GetValue([NotNull] SoftString settingName, [NotNull] Type settingType, [CanBeNull] SoftString dataStoreName);

        void SetValue([NotNull] SoftString settingName, [CanBeNull] object value);
    }
}