using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.IOnymous;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    using static ValueProviderMetadataKeyNames;

    public class AppSettingProvider : SettingProvider2
    {
        public AppSettingProvider()
            : base(
                ResourceProviderMetadata.Empty
                    .Add(CanGet, true)
                    .Add(CanPut, true)
            )
        { }

        public override Task<IResourceInfo> GetAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            Assert(uri);

            var settingName = new SettingName(uri);
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingName) ?? settingName;
            var element = exeConfig.AppSettings.Settings[actualKey];
            return Task.FromResult<IResourceInfo>(new AppSettingResourceInfo(uri, element?.Value));
        }

        public override async Task<IResourceInfo> PutAsync(SimpleUri uri, Stream value, ResourceProviderMetadata metadata = null)
        {
            using (var valueReader = new StreamReader(value))
            {
                return await PutAsync(uri, await valueReader.ReadToEndAsync());
            }
        }

        public override Task<IResourceInfo> PutAsync(SimpleUri uri, object value, ResourceProviderMetadata metadata = null)
        {
            Assert(uri);

            var settingName = new SettingName(uri);
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingName) ?? settingName;
            var element = exeConfig.AppSettings.Settings[actualKey];

            if (element is null)
            {
                exeConfig.AppSettings.Settings.Add(settingName, (string)value);
            }
            else
            {
                exeConfig.AppSettings.Settings[actualKey].Value = (string)value;
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return GetAsync(actualKey);
        }

        public override Task<IResourceInfo> DeleteAsync(SimpleUri uri, ResourceProviderMetadata metadata = null)
        {
            throw new NotImplementedException();
        }

        [CanBeNull]
        private static string FindActualKey(System.Configuration.Configuration exeConfig, string key)
        {
            return
                exeConfig
                    .AppSettings
                    .Settings
                    .AllKeys
                    .FirstOrDefault(k => SoftString.Comparer.Equals(k, key));
        }
    }

    internal class AppSettingResourceInfo : ResourceInfo
    {
        [CanBeNull]
        private readonly string _value;

        internal AppSettingResourceInfo([NotNull] SimpleUri uri, [CanBeNull] string value) : base(uri)
        {
            _value = value;
        }

        public override bool Exists => !(_value is null);

        public override long? Length => _value?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        public override async Task CopyToAsync(Stream stream)
        {
            if (Exists)
            {
                // ReSharper disable once AssignNullToNotNullAttribute - this isn't null here
                using (var valueStream = _value.ToStreamReader())
                {
                    await valueStream.BaseStream.CopyToAsync(stream);
                }
            }
        }

        public override Task<object> DeserializeAsync(Type targetType)
        {
            return Task.FromResult<object>(_value);
        }
    }


    public class AppSettings : SettingProvider
    {
        public AppSettings(ISettingConverter converter) : base(new SettingNameFactory(), converter) { }

        protected override ISetting Read(SettingName name)
        {
            var exeConfig = OpenExeConfiguration();
            var element = exeConfig.AppSettings.Settings[name.ToString()];
            return element is null ? default : new Setting(name, element.Value);
        }

        protected override void Write(ISetting setting)
        {
            var exeConfig = OpenExeConfiguration();

            if (exeConfig.AppSettings.Settings.AllKeys.Any(k => setting.Name.Equals(k)))
            {
                exeConfig.AppSettings.Settings[setting.ToString()].Value = setting.Value?.ToString();
            }
            else
            {
                exeConfig.AppSettings.Settings.Add(setting.Name.ToString(), (string)setting.Value);
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);
        }

        private static System.Configuration.Configuration OpenExeConfiguration()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }
    }
}