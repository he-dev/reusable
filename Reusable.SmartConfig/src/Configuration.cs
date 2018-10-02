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
        private readonly IEnumerable<ISettingProvider> _providers;

        private readonly ISettingFinder _settingFinder;

        private readonly IDictionary<SoftString, (SoftString ActualName, ISettingProvider SettingProvider)> _settingMap = new Dictionary<SoftString, (SoftString ActualName, ISettingProvider SettingProvider)>();

        private static readonly IDuckValidator<IEnumerable<ISettingProvider>> SettingProviderValidator = new DuckValidator<IEnumerable<ISettingProvider>>(builder =>
        {
            builder
                .IsNotValidWhen(providers => providers == null, DuckValidationRuleOptions.BreakOnFailure)
                .IsValidWhen(providers => providers.Any(), _ => "You need to specify at least one setting-provider.");
        });

        public Configuration([NotNull, ItemNotNull] IEnumerable<ISettingProvider> dataStores, [NotNull] ISettingFinder settingFinder)
        {
            // ReSharper disable once ConstantConditionalAccessQualifier - yes, this can be null
            _providers = (dataStores?.ToList()).ValidateWith(SettingProviderValidator).ThrowOrDefault();
            _settingFinder = settingFinder ?? throw new ArgumentNullException(nameof(settingFinder));
        }

        public Configuration([NotNull, ItemNotNull] IEnumerable<ISettingProvider> dataStores)
            : this(dataStores, new FirstSettingFinder())
        {
        }

        public object GetValue(SoftString settingName, Type settingType, SoftString dataStoreName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            if (_settingFinder.TryFindSetting(_providers, settingName, settingType, dataStoreName, out var result))
            {
                CacheSettingName(settingName, result.Setting.Name, result.SettingProvider);
                return result.Setting.Value;
            }
            else
            {
                throw ("SettingNotFound", $"Setting {settingName.ToString().QuoteWith("'")} not found.").ToDynamicException();
            }
        }

        private void CacheSettingName(SoftString settingName, SoftString settingActualName, ISettingProvider settingProvider)
        {
            _settingMap[settingName] = (settingActualName, settingProvider);
        }

        public void SetValue(SoftString settingName, object value, SoftString providerName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            if (_settingMap.TryGetValue(settingName, out var item))
            {
                item.SettingProvider.Write(new Setting(item.ActualName, value));
            }
            else
            {
                if (_settingFinder.TryFindSetting(_providers, settingName, null, providerName, out var result))
                {
                    CacheSettingName(settingName, result.Setting.Name, result.SettingProvider);
                    SetValue(settingName, value, providerName);
                }
                else
                {
                    var dataStore = _providers.FirstOrDefault(x => providerName is null || x.Name.Equals(providerName));
                    if (dataStore is null)
                    {
                        throw 
                        (
                            $"SettingDataStoreNotFound",
                            $"Could not find setting data store {providerName?.ToString().QuoteWith("'")}"
                        ).ToDynamicException();
                    }

                    var defaultSettingName = dataStore.SettingNameGenerator.GenerateSettingNames(settingName).First();
                    dataStore.Write(new Setting(defaultSettingName, value));
                }

                //throw ("SettingNotInitializedException", $"Setting {settingName.ToString().QuoteWith("'")} needs to be initialized before you can update it.").ToDynamicException();
            }
        }
    }
}