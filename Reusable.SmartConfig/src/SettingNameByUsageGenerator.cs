using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    /*

    Setting names are ordered by the usage frequency.

    Type.Property,Instance
    Property,Instance
    Namespace+Type.Property,Instance

    Type.Property
    Property
    Namespace+Type.Property

     */
    public class SettingNameByUsageGenerator : ISettingNameGenerator
    {
        private static readonly IEnumerable<Func<SettingName, SettingName>> SettingNameFactories = new Func<SettingName, SettingName>[]
        {
            source => new SettingName(source.Member) { Type = source.Type, Instance = source.Instance},
            source => new SettingName(source.Member) { Instance = source.Instance },
            source => new SettingName(source.Member) { Namespace = source.Namespace, Type = source.Type, Instance = source.Instance }
        };

        public IEnumerable<SettingName> GenerateSettingNames(SoftString settingName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            var localSettingName = SettingName.Parse(settingName.ToString());
            return
                Enumerable.Concat(
                    GenerateSettingNamesWithInstance(localSettingName),
                    GenerateSettingNamesWithoutInstance(localSettingName)
                )
                .Distinct();
        }

        private static IEnumerable<SettingName> GenerateSettingNamesWithInstance(SettingName settingName)
        {
            return
                settingName.Instance.IsNullOrEmpty()
                    ? Enumerable.Empty<SettingName>()
                    : SettingNameFactories.Select(factory => factory(settingName));
        }

        private static IEnumerable<SettingName> GenerateSettingNamesWithoutInstance(SettingName settingName)
        {
            var settingNameWithoutInstance = new SettingName(settingName.Member)
            {
                Namespace = settingName.Namespace,
                Type = settingName.Type
            };

            return SettingNameFactories.Select(factory => factory(settingNameWithoutInstance));
        }
    }
}