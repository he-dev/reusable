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
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Converters;

// ReSharper disable once CheckNamespace
namespace Reusable.IOnymous.Controllers
{
    public class JsonConfigurationController : SettingController
    {
        private readonly IConfiguration _configuration;

        public JsonConfigurationController(string basePath, string fileName) : base(ImmutableContainer.Empty)
        {
            _configuration =
                new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(fileName)
                    .Build();
        }

        public ITypeConverter ResourceConverter { get; set; } = new JsonSettingConverter();

        [ResourceGet]
        public Task<Response> GetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var data = _configuration[settingIdentifier];

            return
                data is null
                    ? new Response.NotFound().ToTask<Response>()
                    : new Response.OK()
                    {
                        Body = data,
                        ContentType = MimeType.Json,
                        Metadata = request
                            .Metadata
                            .Copy<ResourceProperties>()
                            //.SetItem(ResourceProperties.Uri, request.Uri)
                            .SetItem(SettingControllerProperties.Converter, ResourceConverter)
                    }.ToTask<Response>();
        }
    }
}