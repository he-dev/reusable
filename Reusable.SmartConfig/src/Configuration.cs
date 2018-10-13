using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;
using Reusable.Validation;

namespace Reusable.SmartConfig
{
    [PublicAPI]
    public interface IConfiguration
    {
        [CanBeNull]
        object GetValue([NotNull] SelectQuery query);

        void SetValue([NotNull] UpdateQuery query);
    }

    public class Configuration : IConfiguration
    {
        private readonly IEnumerable<ISettingProvider> _providers;

        private readonly ISettingFinder _settingFinder;

        private readonly IDictionary<SettingName, SoftString> _settingProviderNames = new Dictionary<SettingName, SoftString>();

        private static readonly IWeelidator<IEnumerable<ISettingProvider>> SettingProviderWeelidator = Weelidator.For<IEnumerable<ISettingProvider>>(
            builder =>
            {
                builder.BlockNull();
                builder.Ensure(providers => providers.Any()).WithMessage("You need to specify at least one setting-provider.");
            }
        );

        public Configuration([NotNull][ItemNotNull] IEnumerable<ISettingProvider> settingProviders, [NotNull] ISettingFinder settingFinder)
        {
            if (settingProviders == null) throw new ArgumentNullException(nameof(settingProviders));

            _providers = settingProviders.ToList().ValidateWith(SettingProviderWeelidator).ThrowIfInvalid();
            _settingFinder = settingFinder ?? throw new ArgumentNullException(nameof(settingFinder));
        }

        public Configuration([NotNull][ItemNotNull] IEnumerable<ISettingProvider> settingProviders)
            : this(settingProviders, new FirstSettingFinder())
        {
        }

        public object GetValue(SelectQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            query.ProviderName = _settingProviderNames.TryGetValue(query.SettingName, out var providerName) ? providerName : query.ProviderName;

            try
            {
                if (_settingFinder.TryFindSetting(query, _providers, out var result))
                {
                    CacheProvider(query.SettingName, result.SettingProvider.Name);
                    return result.Setting.Value;
                }
            }
            catch (Exception ex)
            {
                throw ($"{nameof(GetValue)}", $"Could not get value for {query.SettingName.ToString().QuoteWith("'")}. See the inner exception for details.", ex).ToDynamicException();
            }

            throw ("SettingNotFound", $"Setting {query.SettingName.ToString().QuoteWith("'")} not found.").ToDynamicException();
        }

        public void SetValue(UpdateQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var providerName =
                query.ProviderName
                ?? (_settingProviderNames.TryGetValue(query.SettingName, out var pn)
                    ? pn
                    : throw DynamicException.Create(
                        "MissingSettingProviderName",
                        $"You need to specify a provider for '{query.SettingName}'."
                    )
                );

            _providers
                .Single(p => p.Name == providerName)
                .Write(query);

            CacheProvider(query.SettingName, providerName);
        }

        private void CacheProvider(SettingName settingName, SoftString providerName)
        {
            _settingProviderNames[settingName] = providerName;
        }
    }
}