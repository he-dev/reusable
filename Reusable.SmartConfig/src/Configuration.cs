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

        private readonly IDictionary<SettingName, SoftString> _settingProviders = new Dictionary<SettingName, SoftString>();

        private static readonly IDuckValidator<IEnumerable<ISettingProvider>> SettingProviderValidator = new DuckValidator<IEnumerable<ISettingProvider>>(
            builder =>
            {
                builder
                    .IsNotValidWhen(providers => providers == null, DuckValidationRuleOptions.BreakOnFailure)
                    .IsValidWhen(providers => providers.Any(), _ => "You need to specify at least one setting-provider.");
            }
        );

        public Configuration([NotNull][ItemNotNull] IEnumerable<ISettingProvider> dataStores, [NotNull] ISettingFinder settingFinder)
        {
            // ReSharper disable once ConstantConditionalAccessQualifier - yes, this can be null
            _providers = (dataStores?.ToList()).ValidateWith(SettingProviderValidator).ThrowOrDefault();
            _settingFinder = settingFinder ?? throw new ArgumentNullException(nameof(settingFinder));
        }

        public Configuration([NotNull][ItemNotNull] IEnumerable<ISettingProvider> dataStores)
            : this(dataStores, new FirstSettingFinder()) { }

        public object GetValue(GetValueQuery getValueQuery)
        {
            if (getValueQuery == null) throw new ArgumentNullException(nameof(getValueQuery));

            if (getValueQuery.ProviderName is null && _settingProviders.TryGetValue(getValueQuery.SettingName, out var providerName))
            {
                getValueQuery.ProviderName = providerName;
            }

            if (_settingFinder.TryFindSetting(getValueQuery, _providers, out var result))
            {
                CacheProvider(getValueQuery.SettingName, result.SettingProvider.Name);
                return result.Setting.Value;
            }
            else
            {
                throw ("SettingNotFound", $"Setting {getValueQuery.SettingName.ToString().QuoteWith("'")} not found.").ToDynamicException();
            }
        }        

        public void SetValue(SetValueQuery setValueQuery)
        {
            if (setValueQuery == null) throw new ArgumentNullException(nameof(setValueQuery));

            if (_settingProviders.TryGetValue(setValueQuery.SettingName, out var providerName))
            {
                _providers.Single(p => p.Name == providerName).Write(setValueQuery.SettingName, setValueQuery.Value, setValueQuery.SettingNameConvention);
            }
            else
            {
                // if (_settingFinder.TryFindSetting(_providers, settingName, null, providerName, out var result))
                // {
                //     CacheProvider(settingName, result.Setting.Name, result.SettingProvider);
                //     SetValue(settingName, value, providerName);
                // }
                // else
                // {
                //     var dataStore = _providers.FirstOrDefault(x => providerName is null || x.Name.Equals(providerName));
                //     if (dataStore is null)
                //     {
                //         throw
                //         (
                //             $"SettingDataStoreNotFound",
                //             $"Could not find setting data store {providerName?.ToString().QuoteWith("'")}"
                //         ).ToDynamicException();
                //     }
                //
                //     var defaultSettingName = dataStore.SettingNameGenerator.GenerateSettingNames(SettingName.Parse(settingName.ToString())).First();
                //     dataStore.Write(new Setting(defaultSettingName, value));
                // }

                //throw ("SettingNotInitializedException", $"Setting {settingName.ToString().QuoteWith("'")} needs to be initialized before you can update it.").ToDynamicException();
            }
        }
        
        private void CacheProvider(SettingName settingName, SoftString providerName)
        {
            _settingProviders[settingName] = providerName;
        }
    }
}