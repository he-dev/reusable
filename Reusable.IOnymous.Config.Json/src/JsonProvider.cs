using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.IOnymous.Config
{
    public class JsonProvider : SettingProvider
    {
        private readonly IConfiguration _configuration;

        public JsonProvider(string basePath, string fileName) : base(ImmutableContainer.Empty)
        {
            _configuration =
                new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(fileName)
                    .Build();
            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Get, GetAsync);
        }

        public ITypeConverter UriConverter { get; set; } = UriStringQueryToStringConverter.Default;

        public ITypeConverter ResourceConverter { get; set; } = new JsonSettingConverter();

        private Task<IResource> GetAsync(Request request)
        {
            var settingIdentifier = UriConverter?.Convert<string>(request.Uri) ?? request.Uri;
            var data = _configuration[settingIdentifier];
            var result =
                data is null
                    ? Resource.DoesNotExist.FromRequest(request)
                    : new JsonResource
                    (
                        data,
                        request
                            .Context
                            .CopyResourceProperties()
                            .SetUri(request.Uri)
                            .SetItem(SettingProperty.Converter, ResourceConverter)
                    );
            return result.ToTask();
        }
    }
}