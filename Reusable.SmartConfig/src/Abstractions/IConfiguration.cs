using System;
using JetBrains.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    [PublicAPI]
    public interface IConfiguration
    {
        [CanBeNull]
        object GetValue([NotNull] GetValueQuery getValueQuery);

        void SetValue([NotNull] SetValueQuery setValueQuery);
    }

    public class GetValueQuery
    {
        public GetValueQuery(SettingName settingName, Type settingType)
        {
            SettingName = settingName;
            SettingType = settingType;
        }
        
        [NotNull]
        public SettingName SettingName { get; }              

        [NotNull]
        public Type SettingType { get; }

        [CanBeNull]
        public SoftString Instance { get; set; }

        [CanBeNull]
        public SoftString ProviderName { get; set; }

        [CanBeNull]
        public SettingNameConvention? SettingNameConvention { get; set; }
    }
    
    public class SetValueQuery
    {
        public SetValueQuery(SettingName settingName, object value)
        {
            SettingName = settingName;
            Value = value;
        }
        
        [NotNull]
        public SettingName SettingName { get; }

        [CanBeNull]
        public object Value { get; }

        [CanBeNull]
        public SoftString ProviderName { get; set; }

        [CanBeNull]
        public SettingNameConvention? SettingNameConvention { get; set; }
    }
}