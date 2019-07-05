using System;
using System.Configuration;
using System.IO;
using System.Linq;
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
        public AppSettingProvider() : base(ImmutableContainer.Empty)
        {
            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Get, GetAsync)
                    .Add(RequestMethod.Put, PutAsync);
        }

        [CanBeNull]
        public ITypeConverter UriConverter { get; set; } = DefaultUriStringConverter;

        public ITypeConverter ValueConverter { get; set; } = new NullConverter();

        private Task<IResource> GetAsync(Request request)
        {
            var settingIdentifier = UriConverter?.Convert<string>(request.Uri) ?? request.Uri;
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];
            return Task.FromResult<IResource>(
                new AppSetting(
                    ImmutableContainer
                        .Empty
                        .SetItem(Resource.PropertySelector.Select(x => x.ActualName), settingIdentifier)
                        .Union(request.Properties.Copy(Resource.PropertySelector)),
                    element?.Value));
        }

        private async Task<IResource> PutAsync(Request request)
        {
            var settingIdentifier = UriConverter?.Convert<string>(request.Uri) ?? request.Uri;
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, settingIdentifier) ?? settingIdentifier;
            var element = exeConfig.AppSettings.Settings[actualKey];

            var value = await ResourceHelper.Deserialize<object>(request.Body, request.Properties.Copy(Resource.PropertySelector));
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

            return await GetAsync(request);
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

    internal class AppSetting : Resource
    {
        [CanBeNull]
        private readonly string _value;

        // This is always Text
        internal AppSetting(IImmutableContainer properties, [CanBeNull] string value)
            : base(properties
                .SetItem(PropertySelector.Select(x => x.Format), MimeType.Text)
                .SetItem(PropertySelector.Select(x => x.Exists), value.IsNotNullOrEmpty()))
        {
            _value = value;
        }

        //public override bool Exists => !(_value is null);

        //public override long? Length => _value?.Length;

        public override async Task CopyToAsync(Stream stream)
        {
            //this.ValidateWith(Validations.Exists).ThrowIfNotValid();

            // ReSharper disable once AssignNullToNotNullAttribute - this isn't null here
            using (var valueStream = _value.ToStreamReader())
            {
                await valueStream.BaseStream.CopyToAsync(stream);
            }
        }
    }
}