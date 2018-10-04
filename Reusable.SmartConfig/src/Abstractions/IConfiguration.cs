using System;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig
{
    [PublicAPI]
    public interface IConfiguration
    {
        [CanBeNull]
        object GetValue([NotNull] GetValueQuery query);

        void SetValue([NotNull] SetValueQuery query);
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
        public SoftString ProviderName { get; set; }

        [CanBeNull]
        public SettingNameComplexity? SettingNameComplexity { get; set; }
        
        [CanBeNull]
        public bool? PrefixEnabled { get; set; }

        public static GetValueQuery Create(SettingInfo settingInfo, string instance)
        {
            var settingName = new SettingName
            (
                prefix: settingInfo.Prefix, //.Type.Assembly.GetCustomAttribute<SettingAssemblyAttribute>()?.Name ?? Type.Assembly.GetName().Name,
                schema: settingInfo.Schema,
                type: settingInfo.TypeName,
                member: settingInfo.MemberName,
                instance: instance
            );
            
            return new GetValueQuery(settingName, settingInfo.Type)
            {
                ProviderName = settingInfo.ProviderName,
                SettingNameComplexity = settingInfo.SettingNameComplexity, 
                PrefixEnabled = settingInfo.PrefixEnabled,
            };
            
        }
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