using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Stratus
{
    public class ConnectionStringProvider : IValueProvider
    {
        public ValueProviderCapabilities Capabilities => ValueProviderCapabilities.CanReadValue | ValueProviderCapabilities.CanWriteValue;

        public Task<IValueInfo> GetValueInfoAsync(string name)
        {
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, name) ?? name;
            var element = exeConfig.AppSettings.Settings[actualKey];
            return Task.FromResult<IValueInfo>(new ConnectionStringInfo(element is null ? name : actualKey, element?.Value));
        }

        public async Task<IValueInfo> WriteValueAsync(string name, Stream value)
        {
            if (value is null)
            {
                return await GetValueInfoAsync(name);
            }
            else
            {
                using (var valueReader = new StreamReader(value))
                {
                    return await SerializeValueAsync(name, await valueReader.ReadToEndAsync());
                }
            }
        }

        public Task<IValueInfo> SerializeValueAsync(string name, object value)
        {
            var exeConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var actualKey = FindActualKey(exeConfig, name) ?? name;
            var element = exeConfig.AppSettings.Settings[actualKey];

            if (element is null)
            {
                exeConfig.AppSettings.Settings.Add(name, (string)value);
            }
            else
            {
                exeConfig.AppSettings.Settings[actualKey].Value = (string)value;
            }

            exeConfig.Save(ConfigurationSaveMode.Minimal);

            return GetValueInfoAsync(actualKey);
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

    internal class ConnectionStringInfo : ValueInfo
    {
        [CanBeNull]
        private readonly string _value;

        internal ConnectionStringInfo([NotNull] string name, [CanBeNull] string value) : base(name)
        {
            _value = value;
        }

        public override bool Exists => !(_value is null);

        public override long Length => _value?.Length ?? OutOfRange;

        public override DateTime? ModifiedOn { get; }

        public override ValueInfoType Type => "AppSetting";

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

        public override Task<object> DeserializeAsync()
        {
            return Task.FromResult<object>(_value);
        }
    }
}
