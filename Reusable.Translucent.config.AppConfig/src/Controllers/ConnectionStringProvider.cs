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
    public class ConnectionStringProvider : SettingProvider
    {
        public ConnectionStringProvider() : base(ImmutableSession.Empty)
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
            var settings = FindConnectionStringSettings(exeConfig, settingIdentifier);
            return Task.FromResult<IResource>(new ConnectionString(request.Uri, settings?.ConnectionString));
        }

        private async Task<IResource> PutAsync(Request request)
        {
            using (var valueReader = new StreamReader(stream))
            {
                var value = await valueReader.ReadToEndAsync();

                var settingIdentifier = UriConverter?.Convert<string>(request.Uri) ?? request.Uri;
                var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = FindConnectionStringSettings(exeConfig, settingIdentifier);

                if (settings is null)
                {
                    exeConfig.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(settingIdentifier, value));
                }
                else
                {
                    settings.ConnectionString = value;
                }

                exeConfig.Save(ConfigurationSaveMode.Minimal);

                return await GetAsync(request.Uri);
            }
        }

        [CanBeNull]
        private static ConnectionStringSettings FindConnectionStringSettings(System.Configuration.Configuration exeConfig, string key)
        {
            return
                exeConfig
                    .ConnectionStrings
                    .ConnectionStrings
                    .Cast<ConnectionStringSettings>()
                    .SingleOrDefault(x => SoftString.Comparer.Equals(x.Name, key));
        }
    }

    internal class ConnectionString : Resource
    {
        [CanBeNull]
        private readonly string _value;

        internal ConnectionString([NotNull] UriString uri, [CanBeNull] string value)
            : base(uri, ImmutableSession.Empty.SetItem(From<IResourceMeta>.Select(x => x.Format), MimeType.Text))
        {
            _value = value;
        }

        //public override bool Exists => !(_value is null);

        //public override long? Length => _value?.Length;
        
        public override async Task CopyToAsync(Stream stream)
        {
            // ReSharper disable once AssignNullToNotNullAttribute - this isn't null here
            using (var valueStream = _value.ToStreamReader())
            {
                await valueStream.BaseStream.CopyToAsync(stream);
            }
        }
    }
}