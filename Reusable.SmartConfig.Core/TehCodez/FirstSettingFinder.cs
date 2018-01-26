using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public class FirstSettingFinder : ISettingFinder
    {
        public bool TryFindSetting(
            IEnumerable<ISettingDataStore> dataStores,
            SoftString settingName,
            Type settingType,
            SoftString dataStoreName,
            out (ISettingDataStore DataStore, ISetting Setting) result)
        {
            if (dataStores == null) throw new ArgumentNullException(nameof(dataStores));
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            var anyDataStore = dataStoreName.IsNullOrEmpty();
            var settingQuery =
                from datastore in dataStores
                where anyDataStore || datastore.Name.Equals(dataStoreName)
                let setting = datastore.Read(settingName, settingType)
                where setting != null
                select new { Datastore = datastore, Setting =  setting};

            var first = settingQuery.FirstOrDefault();

            if (first is default)
            {
                result = default((ISettingDataStore, ISetting));
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