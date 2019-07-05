using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Reusable.Data;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.IOnymous.Config
{
    public class JsonProvider : SettingProvider
    {
        private readonly IConfiguration _configuration;

        public JsonProvider(string fileName) : base(ImmutableSession.Empty)
        {
            _configuration =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
        }

        public ITypeConverter UriConverter { get; set; } = DefaultUriStringConverter;

        public ITypeConverter ValueConverter { get; set; } = new JsonSettingConverter();

        protected override Task<IResource> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            var settingIdentifier = UriConverter?.Convert<string>(uri) ?? uri;

            var data = _configuration[settingIdentifier];
            if (data is null)
            {
                return Task.FromResult<IResource>(new JsonResource(uri, default, metadata));
            }
            else
            {
                var value = ValueConverter.Convert(data, metadata.GetItemOrDefault(From<IResourceMeta>.Select(x => x.Type)));
                metadata = ImmutableSession.Empty.SetItem(From<IResourceMeta>.Select(x => x.ActualName), settingIdentifier);
                return Task.FromResult<IResource>(new JsonResource(uri, value, metadata));
            }
        }
    }

    internal class JsonResource : Resource
    {
        [CanBeNull] private readonly object _value;

        internal JsonResource([NotNull] UriString uri, [CanBeNull] object value, IImmutableSession metadata)
            : base(uri, metadata.SetItem(From<IResourceMeta>.Select(x => x.Format), value is string ? MimeType.Text : MimeType.Binary))
        {
            _value = value;
        }

        public override bool Exists => !(_value is null);

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            var format = Metadata.GetItemOrDefault(From<IResourceMeta>.Select(x => x.Format));
            if (format == MimeType.Text)
            {
                using (var s = await ResourceHelper.SerializeTextAsync((string)_value))
                {
                    await s.Rewind().CopyToAsync(stream);
                }
            }

            if (format == MimeType.Binary)
            {
                using (var s = await ResourceHelper.SerializeBinaryAsync(_value))
                {
                    await s.Rewind().CopyToAsync(stream);
                }
            }
        }
    }
}