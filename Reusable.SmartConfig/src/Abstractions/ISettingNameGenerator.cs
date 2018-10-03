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
        IEnumerable<SettingName> GenerateSettingNames([NotNull] SettingName settingName);

        //IEnumerable<IList<string>> GenerateSettingNames(IEnumerable<string> names);
    }
}