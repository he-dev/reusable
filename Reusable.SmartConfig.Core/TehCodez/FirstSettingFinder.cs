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
        public (ISettingDataStore DataStore, ISetting Setting) FindSetting(
            IEnumerable<ISettingDataStore> dataStores, 
            SoftString settingName, 
            Type settingType, 
            SoftString dataStoreName)
        {
            if (dataStores == null) throw new ArgumentNullException(nameof(dataStores));
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));
            if (settingType == null) throw new ArgumentNullException(nameof(settingType));
            if (dataStoreName == null) throw new ArgumentNullException(nameof(dataStoreName));

            var anyDataStore = dataStoreName.IsNullOrEmpty();
            var settingQuery =
                from datastore in dataStores
                where  anyDataStore || datastore.Name.Equals(dataStoreName)
                select (Datastore: datastore, Setting: datastore.Read(settingName, settingType));

            var setting = settingQuery.FirstOrDefault(t => t.Setting.IsNotNull());

            if (setting.Datastore.IsNull())
            {
                throw ("SettingNotFoundException", $"Setting {settingName.ToString().QuoteWith("'")} not found.").ToDynamicException();
            }

            return setting;
        }
    }
}