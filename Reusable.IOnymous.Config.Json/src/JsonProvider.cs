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
        }

        public ITypeConverter ResourceConverter { get; set; } = new JsonSettingConverter();

        [ResourceGet]
        public Task<IResource> GetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var data = _configuration[settingIdentifier];
            var result =
                data is null
                    ? DoesNotExist(request)
                    : new JsonResource
                    (
                        data,
                        request
                            .Context
                            .Copy<ResourceProperties>()
                            .SetItem(ResourceProperties.Uri, request.Uri)
                            .SetItem(SettingProperty.Converter, ResourceConverter)
                    );
            return result.ToTask();
        }
    }
}