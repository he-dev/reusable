using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;
using Reusable.SmartConfig.Data;
using Reusable.Validation;

namespace Reusable.SmartConfig
{
    public class Configuration : IConfiguration
    {
        private readonly IEnumerable<ISettingDataStore> _dataStores;

        private readonly ISettingFinder _settingFinder;

        private readonly IDictionary<SoftString, (SoftString ActualName, ISettingDataStore Datastore)> _settingMap = new Dictionary<SoftString, (SoftString ActualName, ISettingDataStore Datastore)>();

        private static readonly IValidator<IEnumerable<ISettingDataStore>> DataStoresValidator =
            Validator
                .Create<IEnumerable<ISettingDataStore>>()
                .IsNotValidWhen(dataStores => dataStores == null, ValidationOptions.StopOnFailure)
                .IsValidWhen(x => x.Any(), _ => "You need to specify at least one data-store.");

        public Configuration([NotNull, ItemNotNull] IEnumerable<ISettingDataStore> dataStores, [NotNull] ISettingFinder settingFinder)
        {
            // ReSharper disable once ConstantConditionalAccessQualifier - yes, this can be null
            _dataStores = (dataStores?.ToList()).ValidateWith(DataStoresValidator).ThrowIfNotValid();
            _settingFinder = settingFinder ?? throw new ArgumentNullException(nameof(settingFinder));
        }

        public Configuration([NotNull, ItemNotNull] IEnumerable<ISettingDataStore> dataStores)
            : this(dataStores, new FirstSettingFinder())
        { }

        public object GetValue(SoftString settingName, Type settingType, SoftString dataStoreName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            if (_settingFinder.TryFindSetting(_dataStores, settingName, settingType, dataStoreName, out var result))
            {
                CacheSettingName(settingName, result.Setting.Name, result.DataStore);
                return result.Setting.Value;
            }
            else
            {
                throw ("SettingNotFoundException", $"Setting {settingName.ToString().QuoteWith("'")} not found.").ToDynamicException();
            }
        }

        private void CacheSettingName(SoftString settingName, SoftString settingActualName, ISettingDataStore settingDataStore)
        {
            _settingMap[settingName] = (settingActualName, settingDataStore);
        }

        public void SetValue(SoftString settingName, object value, SoftString dataStoreName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            if (_settingMap.TryGetValue(settingName, out var item))
            {
                item.Datastore.Write(new Setting(item.ActualName) { Value = value });
            }
            else
            {
                if (_settingFinder.TryFindSetting(_dataStores, settingName, null, dataStoreName, out var result))
                {
                    CacheSettingName(settingName, result.Setting.Name, result.DataStore);
                    SetValue(settingName, value, dataStoreName);
                }
                else
                {
                    var dataStore = _dataStores.FirstOrDefault(x => dataStoreName is null || x.Name.Equals(dataStoreName));
                    if (dataStore is null)
                    {
                        throw DynamicException.Factory.CreateDynamicException(
                            $"SettingDataStoreNotFound{nameof(Exception)}",
                            $"Could not find setting data store {dataStoreName?.ToString().QuoteWith("'")}",
                            null
                        );
                    }

                    var defaultSettingName = dataStore.SettingNameGenerator.GenerateSettingNames(settingName).First();
                    dataStore.Write(new Setting(defaultSettingName) { Value = value });
                }
                //throw ("SettingNotInitializedException", $"Setting {settingName.ToString().QuoteWith("'")} needs to be initialized before you can update it.").ToDynamicException();
            }
        }
    }
}