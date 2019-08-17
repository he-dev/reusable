using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Translucent.Converters;

namespace Reusable.Translucent.Controllers
{
    public class JsonFileController : ConfigController
    {
        private readonly IConfiguration _configuration;

        public JsonFileController(string basePath, string fileName) 
            : base(ImmutableContainer.Empty.SetItem(Converter, new JsonSettingConverter()))
        {
            _configuration =
                new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(fileName)
                    .Build();
        }
        
        [ResourceGet]
        public Task<Response> GetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var data = _configuration[settingIdentifier];

            return
                data is null
                    ? new Response.NotFound().ToTask<Response>()
                    : OK(request, data, settingIdentifier).ToTask();
        }
    }
}