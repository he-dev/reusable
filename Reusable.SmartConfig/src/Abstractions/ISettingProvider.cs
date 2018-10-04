using System;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingProvider : IEquatable<ISettingProvider>
    {
        [NotNull]
        [AutoEqualityProperty]
        SoftString Name { get; }        

        [CanBeNull]
        ISetting Read([NotNull] SettingName settingName, [NotNull] Type settingType, SettingNameConvention? settingNameConvention = default);

        void Write([NotNull] SettingName settingName, [CanBeNull] object value, SettingNameConvention? settingNameConvention = default);
    }
}