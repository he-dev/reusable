using System;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingDataStore : IEquatable<ISettingDataStore>
    {
        [NotNull]
        [AutoEqualityProperty]
        SoftString Name { get; }

        [NotNull]
        ISettingNameGenerator SettingNameGenerator { get; }

        [CanBeNull]
        ISetting Read([NotNull] SoftString settingName, [CanBeNull] Type settingType);

        void Write([NotNull] ISetting setting);
    }
}