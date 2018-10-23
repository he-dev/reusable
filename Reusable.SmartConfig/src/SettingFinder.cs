using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingFinder
    {
        bool TryFindSetting
        (
            [NotNull] SelectQuery query,
            [NotNull] IEnumerable<ISettingProvider> providers,
            out (ISettingProvider SettingProvider, ISetting Setting) result
        );
    }

    /// <summary>
    /// The same setting can be defined in multiple data-stores. This setting-finder picks the first setting it finds.
    /// </summary>
    public class FirstSettingFinder : ISettingFinder
    {
        public bool TryFindSetting
        (
            SelectQuery query,
            IEnumerable<ISettingProvider> providers,
            out (ISettingProvider SettingProvider, ISetting Setting) result
        )
        {
            if (providers == null) throw new ArgumentNullException(nameof(providers));
            if (query == null) throw new ArgumentNullException(nameof(query));            

            var matchedProviders =
            (
                from provider in providers
                where
                    (query.ProviderName is null || provider.Name == query.ProviderName) &&
                    (query.ProviderType is null || provider.GetType() == query.ProviderType)
                select provider
            ).ToList();

            if (matchedProviders.Empty())
            {
                throw DynamicException.Create(
                    "SettingProviderNotFound",
                    $"There is no such provider as {query.ProviderName?.ToString().QuoteWith("'")} or {query.ProviderType?.ToPrettyString().QuoteWith("'")}."
                );
            }

            var findSetting =
                from provider in matchedProviders
                let setting = provider.Read(query)
                where setting.IsNotNull()
                select (provider, setting);

            result = findSetting.FirstOrDefault();
            return !result.IsEmpty();
        }        
    }

    internal static class FirstSettingFinderHelper
    {
        public static bool IsEmpty(this (ISettingProvider SettingProvider, ISetting Setting) result)
        {
            return
                result.SettingProvider is null &&
                result.Setting is null;
        }        
    }
}