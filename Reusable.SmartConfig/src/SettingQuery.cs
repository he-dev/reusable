using System;
using JetBrains.Annotations;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig
{
    public abstract class SettingQuery
    {
        protected SettingQuery(SettingName settingName)
        {
            SettingName = settingName;
        }

        [NotNull]
        public SettingName SettingName { get; }

        [CanBeNull]
        public SoftString ProviderName { get; set; }

        public SettingNameComplexity Complexity { get; set; }

        public PrefixHandling PrefixHandling { get; set; }
    }

    public class SelectQuery : SettingQuery
    {
        public SelectQuery(SettingName settingName, Type settingType) : base(settingName)
        {
            SettingType = settingType;
        }

        [NotNull]
        public Type SettingType { get; }
    }

    public class UpdateQuery : SettingQuery
    {
        public UpdateQuery(SettingName settingName, object value) : base(settingName)
        {
            Value = value;
        }

        [CanBeNull]
        public object Value { get; }
    }

    public static class ValueQueryFactory
    {
        public static SelectQuery CreateGetValueQuery(SettingMetadata settingMetadata, string instance)
        {
            var settingName = new SettingName
            (
                prefix: settingMetadata.Prefix,
                schema: settingMetadata.Schema,
                type: settingMetadata.TypeName,
                member: settingMetadata.MemberName,
                instance: instance
            );

            return new SelectQuery(settingName, settingMetadata.Type)
            {
                ProviderName = settingMetadata.ProviderName,
                Complexity = settingMetadata.SettingNameComplexity,
                PrefixHandling = settingMetadata.PrefixHandling
            };
        }

        public static UpdateQuery CreateSetValueQuery(SettingMetadata settingMetadata, string instance)
        {
            var settingName = new SettingName
            (
                prefix: settingMetadata.Prefix,
                schema: settingMetadata.Schema,
                type: settingMetadata.TypeName,
                member: settingMetadata.MemberName,
                instance: instance
            );

            return new UpdateQuery(settingName, settingMetadata.Type)
            {
                ProviderName = settingMetadata.ProviderName,
                Complexity = settingMetadata.SettingNameComplexity,
                PrefixHandling = settingMetadata.PrefixHandling
            };
        }
    }
}