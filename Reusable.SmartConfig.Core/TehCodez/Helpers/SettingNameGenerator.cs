using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Helpers
{
    public interface ISettingNameGenerator
    {
        /// <summary>
        /// Generates setting names ordered by the usage frequency.
        /// </summary>
        [NotNull, ItemNotNull]
        IEnumerable<SettingName> GenerateSettingNames([NotNull] SettingName fullSettingName);
    }

    /*

    Setting names are ordered by the usage frequency.

    Type.Property,Instance
    Property,Instance
    Namespace+Type.Property,Instance

    Type.Property
    Property
    Namespace+Type.Property

     */
    public class SettingNameGenerator : ISettingNameGenerator
    {
        [NotNull, ItemNotNull]
        private readonly IEnumerable<Func<SettingName, SettingName>> _settingNameFactories;

        public static readonly IEnumerable<Func<SettingName, SettingName>> SettingNamesByUsageFrequency = new Func<SettingName, SettingName>[]
        {
            source => new SettingName(source.Property) { Type = source.Type, Instance = source.Instance},
            source => new SettingName(source.Property) { Instance = source.Instance },
            source => new SettingName(source.Property) { Namespace = source.Namespace, Type = source.Type, Instance = source.Instance }
        };

        public SettingNameGenerator([NotNull, ItemNotNull] IEnumerable<Func<SettingName, SettingName>> settingNameFactories)
        {
            _settingNameFactories = settingNameFactories ?? throw new ArgumentNullException(nameof(settingNameFactories));
        }

        public SettingNameGenerator() : this(SettingNamesByUsageFrequency) { }

        public IEnumerable<SettingName> GenerateSettingNames(SettingName settingName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            return GenerateSettingNamesWithInstance(settingName).Concat(GenerateSettingNamesWithoutInstance(settingName));            
        }

        private IEnumerable<SettingName> GenerateSettingNamesWithInstance(SettingName settingName)
        {
            return
                settingName.Instance.IsNullOrEmpty() 
                    ? Enumerable.Empty<SettingName>() 
                    : _settingNameFactories.Select(factory => factory(settingName));
        }

        private IEnumerable<SettingName> GenerateSettingNamesWithoutInstance(SettingName settingName)
        {
            var settingNameWithoutInstance = new SettingName(settingName.Property)
            {
                Namespace = settingName.Namespace,
                Type = settingName.Type
            };

            return _settingNameFactories.Select(factory => factory(settingNameWithoutInstance));
        }
    }
}