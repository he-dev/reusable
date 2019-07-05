using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Quickey;

namespace Reusable.IOnymous.Config
{
    public class AppSettingProvider : SettingProvider
    {
        public AppSettingProvider() : base(ImmutableSession.Empty) { }

        [CanBeNull]
        public ITypeConverter UriConverter { get; set; } = DefaultUriStringConverter;

        public ITypeConverter ValueConverter { get; set; } = new NullConverter();

        protected override Task<IResource> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            var settingIdentifier = UriConverter?.Convert<string>(uri) ?? uri;
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];
            return Task.FromResult<IResource>(new AppSetting(uri, element?.Value, ImmutableSession.Empty.SetItem(From<IResourceMeta>.Select(x => x.ActualName), settingIdentifier)));
        }

        protected override async Task<IResource> PutAsyncInternal(UriString uri, Stream stream, IImmutableSession metadata)
        {
            var settingIdentifier = UriConverter?.Convert<string>(uri) ?? uri;
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];

            var value = await ResourceHelper.Deserialize<object>(stream, metadata);
            value = ValueConverter.Convert(value, typeof(string));

            if (element is null)
            {
                exeConfig.AppSettings.Settings.Add(settingIdentifier, (string)value);
            }
            else
            {
                exeConfig.AppSettings.Settings[actualKey].Value = (string)value;
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return await GetAsync(uri);
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

    internal class AppSetting : Resource
    {
        [CanBeNull] private readonly string _value;

        internal AppSetting([NotNull] UriString uri, [CanBeNull] string value, IImmutableSession metadata)
            : base(uri, metadata.SetItem(From<IResourceMeta>.Select(x => x.Format), MimeType.Text))
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