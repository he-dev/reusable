using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Quickey;

namespace Reusable.IOnymous.Config
{
    public class AppSettingProvider : SettingProvider
    {
        public AppSettingProvider() : base(ImmutableContainer.Empty) { }

        public ITypeConverter ResourceConverter { get; set; } = new NullConverter();

        [ResourceGet]
        public Task<IResource> GetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];

            var result =
                element is null
                    ? DoesNotExist(request)
                    : new PlainResource
                    (
                        element.Value,
                        request
                            .Context
                            .Copy<ResourceProperties>()
                            .SetItem(SettingProperty.Converter, ResourceConverter)
                            .SetItem(ResourceProperties.ActualName, settingIdentifier)
                    );

            return result.ToTask();
        }

        [ResourcePut]
        public async Task<IResource> SetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];
            var value = ResourceConverter.Convert(request.Body, typeof(string));

            if (element is null)
            {
                exeConfig.AppSettings.Settings.Add(settingIdentifier, (string)value);
            }
            else
            {
                exeConfig.AppSettings.Settings[actualKey].Value = (string)value;
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return await GetSettingAsync(request);
        }

        [CanBeNull]
        private static string FindActualKey(Configuration exeConfig, string key)
        {
            return
                exeConfig
                    .AppSettings
                    .Settings
                    .AllKeys
                    .FirstOrDefault(k => SoftString.Comparer.Equals(k, key));
        }
    }
}