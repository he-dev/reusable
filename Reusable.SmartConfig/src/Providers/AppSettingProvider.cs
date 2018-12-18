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
    using static ResourceMetadataKeys;

    public class AppSettingProvider : ResourceProvider
    {
        public AppSettingProvider()
            : base(
                ResourceMetadata.Empty
                    .Add(CanGet, true)
                    .Add(CanPut, true)
            )
        { }

        public override Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            var settingName = new SettingName(uri);
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingName) ?? settingName;
            var element = exeConfig.AppSettings.Settings[actualKey];
            return Task.FromResult<IResourceInfo>(new AppSettingInfo(uri, element?.Value));
        }

        public override async Task<IResourceInfo> PutAsync(UriString uri, Stream stream, ResourceMetadata metadata = null)
        {
            using (var valueReader = new StreamReader(stream))
            {
                var value = await valueReader.ReadToEndAsync();

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

                return await GetAsync(uri);
            }
        }

        public override Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
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

    internal class AppSettingInfo : ResourceInfo
    {
        [CanBeNull]
        private readonly string _value;

        internal AppSettingInfo([NotNull] UriString uri, [CanBeNull] string value) : base(uri)
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
}