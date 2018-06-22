using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingNameGenerator
    {
        /// <summary>
        /// Generates setting names.
        /// </summary>
        [NotNull, ItemNotNull]
        IEnumerable<SettingName> GenerateSettingNames([NotNull] SoftString settingName);
    }
}