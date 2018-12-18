using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Serialization;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    using static ResourceMetadataKeys;

    public class ConnectionStringProvider : ResourceProvider
    {
        public ConnectionStringProvider()
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
            var settings = FindConnectionStringSettings(exeConfig, settingName);            
            return Task.FromResult<IResourceInfo>(new ConnectionStringInfo(uri, settings?.ConnectionString));
        }

        public override async Task<IResourceInfo> PutAsync(UriString uri, Stream stream, ResourceMetadata metadata = null)
        {
            using (var valueReader = new StreamReader(stream))
            {
                var value = await valueReader.ReadToEndAsync();

                var settingName = new SettingName(uri);
                var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = FindConnectionStringSettings(exeConfig, settingName);

                if (settings is null)
                {
                    exeConfig.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(settingName, value));
                }
                else
                {
                    settings.ConnectionString = value;
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
        private static ConnectionStringSettings FindConnectionStringSettings(Configuration exeConfig, string key)
        {
            return
                exeConfig
                    .ConnectionStrings
                    .ConnectionStrings
                    .Cast<ConnectionStringSettings>()
                    .SingleOrDefault(x => SoftString.Comparer.Equals(x.Name, key));
        }
    }

    internal class ConnectionStringInfo : ResourceInfo
    {
        [CanBeNull]
        private readonly string _value;

        internal ConnectionStringInfo([NotNull] UriString uri, [CanBeNull] string value) : base(uri)
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
