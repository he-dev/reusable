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
            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Get, GetAsync);
        }

        public ITypeConverter UriConverter { get; set; } = DefaultUriStringConverter;

        public ITypeConverter ValueConverter { get; set; } = new JsonSettingConverter();

        private Task<IResource> GetAsync(Request request)
        {
            var settingIdentifier = UriConverter?.Convert<string>(request.Uri) ?? request.Uri;

            var data = _configuration[settingIdentifier];
            if (data is null)
            {
                return Task.FromResult<IResource>(new JsonResource(request.Properties.Copy(Resource.PropertySelector)));
            }
            else
            {
                var value = ValueConverter.Convert(data, request.Properties.GetType());
                //metadata = ImmutableSession.Empty.SetItem(From<IResourceMeta>.Select(x => x.ActualName), settingIdentifier);
                return Task.FromResult<IResource>(new JsonResource(request.Properties.Copy(Resource.PropertySelector), value));
            }
        }
    }

    internal class JsonResource : Resource
    {
        [CanBeNull]
        private readonly object _value;

        internal JsonResource(IImmutableSession properties, object value = default)
            : base(properties
                .SetExists(!(value is null))
                .SetFormat(value is string ? MimeType.Text : MimeType.Binary))
        {
            _value = value;
        }

        //public override bool Exists => !(_value is null);

        public override async Task CopyToAsync(Stream stream)
        {
            var format = Properties.GetItemOrDefault(From<IResourceMeta>.Select(x => x.Format));
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