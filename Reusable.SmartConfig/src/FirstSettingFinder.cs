using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    /// <summary>
    /// The same setting can be defined in multiple data-stores. This setting-finder picks the first setting it finds.
    /// </summary>
    public class FirstSettingFinder : ISettingFinder
    {
        public bool TryFindSetting(
            IEnumerable<ISettingProvider> dataStores,
            SoftString settingName,
            Type settingType,
            SoftString dataStoreName,
            out (ISettingProvider DataStore, ISetting Setting) result)
        {
            if (dataStores == null) throw new ArgumentNullException(nameof(dataStores));
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            var anyDataStore = dataStoreName.IsNullOrEmpty();
            var settingQuery =
                from datastore in dataStores
                where anyDataStore || datastore.Name.Equals(dataStoreName)
                let setting = datastore.Read(settingName, settingType)
                where setting != null
                select new { Datastore = datastore, Setting = setting };

            var first = settingQuery.FirstOrDefault();

            if (first is null)
            {
                result = default((ISettingProvider, ISetting));
                return false;
            }
            else
            {
                result = (first.Datastore, first.Setting);
                return true;
            }
        }
    }
}