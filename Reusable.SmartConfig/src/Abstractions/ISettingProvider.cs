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

        [NotNull]
        ISettingNameGenerator SettingNameGenerator { get; }

        [CanBeNull]
        ISetting Read([NotNull] SoftString settingName, [CanBeNull] Type settingType);

        void Write([NotNull] ISetting setting);
        
        
        //ISetting Read([NotNull] SettingName settingName, [CanBeNull] Type settingType);
        
        //void Write([NotNull] ISetting setting);
    }
}