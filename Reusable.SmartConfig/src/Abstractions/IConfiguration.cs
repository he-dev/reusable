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
        public SettingNameComplexity? SettingNameComplexity { get; set; }

        [CanBeNull]
        public bool? PrefixEnabled { get; set; }
    }

    public static class ValueQueryFactory
    {
        public static GetValueQuery CreateGetValueQuery(SettingMetadata settingMetadata, string instance)
        {
            var settingName = new SettingName
            (
                prefix: settingMetadata.Prefix, //.Type.Assembly.GetCustomAttribute<SettingAssemblyAttribute>()?.Name ?? Type.Assembly.GetName().Name,
                schema: settingMetadata.Schema,
                type: settingMetadata.TypeName,
                member: settingMetadata.MemberName,
                instance: instance
            );

            return new GetValueQuery(settingName, settingMetadata.Type)
            {
                ProviderName = settingMetadata.ProviderName,
                SettingNameComplexity = settingMetadata.SettingNameComplexity,
                PrefixEnabled = settingMetadata.PrefixHandlingEnabled,
            };
        }

        public static SetValueQuery CreateSetValueQuery(SettingMetadata settingMetadata, string instance)
        {
            var settingName = new SettingName
            (
                prefix: settingMetadata.Prefix, //.Type.Assembly.GetCustomAttribute<SettingAssemblyAttribute>()?.Name ?? Type.Assembly.GetName().Name,
                schema: settingMetadata.Schema,
                type: settingMetadata.TypeName,
                member: settingMetadata.MemberName,
                instance: instance
            );

            return new SetValueQuery(settingName, settingMetadata.Type)
            {
                ProviderName = settingMetadata.ProviderName,
                SettingNameComplexity = settingMetadata.SettingNameComplexity,
                PrefixEnabled = settingMetadata.PrefixHandlingEnabled,
            };
        }
    }
}