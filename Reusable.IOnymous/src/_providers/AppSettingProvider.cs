using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OneTo1;

namespace Reusable.IOnymous
{
    public class AppSettingProvider : ResourceProvider
    {
        private readonly ITypeConverter _uriStringToSettingIdentifierConverter;

        public AppSettingProvider(ITypeConverter uriStringToSettingIdentifierConverter = null)
            : base(
                ResourceMetadata.Empty
                    .Add(ResourceMetadataKeys.CanGet, true)
                    .Add(ResourceMetadataKeys.CanPut, true)
                    .Add(ResourceMetadataKeys.Scheme, "setting")
            )
        {
            _uriStringToSettingIdentifierConverter = uriStringToSettingIdentifierConverter;
        }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            var settingIdentifier = (string)_uriStringToSettingIdentifierConverter?.Convert(uri, typeof(string)) ?? uri;
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];
            return Task.FromResult<IResourceInfo>(new AppSettingInfo(uri, element?.Value));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream stream, ResourceMetadata metadata = null)
        {
            using (var valueReader = new StreamReader(stream))
            {
                var value = await valueReader.ReadToEndAsync();

                var settingIdentifier = (string)_uriStringToSettingIdentifierConverter?.Convert(uri, typeof(string)) ?? uri;
                var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
                var element = exeConfig.AppSettings.Settings[actualKey];

                if (element is null)
                {
                    exeConfig.AppSettings.Settings.Add(settingIdentifier, value);
                }
                else
                {
                    exeConfig.AppSettings.Settings[actualKey].Value = value;
                }

                exeConfig.Save(ConfigurationSaveMode.Minimal);

                return await GetAsync(uri);
            }
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

        protected override async Task CopyToAsyncInternal(Stream stream)
        {        
            // ReSharper disable once AssignNullToNotNullAttribute - this isn't null here
            using (var valueStream = _value.ToStreamReader())
            {
                await valueStream.BaseStream.CopyToAsync(stream);
            }
        }

        protected override Task<object> DeserializeAsyncInternal(Type targetType)
        {         
            return Task.FromResult<object>(_value);
        }
    }
}