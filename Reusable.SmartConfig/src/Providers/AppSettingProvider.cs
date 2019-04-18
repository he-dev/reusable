using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;

namespace Reusable.SmartConfig
{
    public class AppSettingProvider : SettingProvider
    {
        public AppSettingProvider() : base(Metadata.Empty) { }

        [CanBeNull]
        public ITypeConverter UriConverter { get; set; } = new UriStringToSettingIdentifierConverter();

        public ITypeConverter Converter { get; set; } = new NullConverter();

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, Metadata metadata)
        {
            var settingIdentifier = UriConverter?.Convert<string>(uri) ?? uri;
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];
            return Task.FromResult<IResourceInfo>(new AppSettingInfo(uri, element?.Value, Metadata.Empty.Resource().InternalName(settingIdentifier)));
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream stream, Metadata metadata)
        {
            using (var valueReader = new StreamReader(stream))
            {
                var value = await valueReader.ReadToEndAsync();

                var settingIdentifier = UriConverter?.Convert<string>(uri) ?? uri;
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
        [CanBeNull] private readonly string _value;

        internal AppSettingInfo([NotNull] UriString uri, [CanBeNull] string value, Metadata metadata)
            : base(uri, m => m.Format(MimeType.Text).Union(metadata))
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
    }
}