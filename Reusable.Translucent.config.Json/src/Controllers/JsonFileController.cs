using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Reusable.Extensions;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Converters;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    /// <summary>
    /// This controller read the new json file from the Microsoft.Extensions.Configuration namespace.
    /// </summary>
    public class JsonFileController : ConfigController
    {
        private readonly IConfiguration _configuration;

        public JsonFileController(ControllerName name, string basePath, string fileName) : base(name)
        {
            _configuration =
                new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(fileName)
                    .Build();

            Converter = new JsonSettingConverter();
        }

        [ResourceGet]
        public Task<Response> GetSettingAsync(ConfigRequest request)
        {
            var settingIdentifier = request.ResourceName;
            var data = _configuration[settingIdentifier];

            return
                data is {}
                    ? OK<ConfigResponse>(data).ToTask<Response>()
                    : NotFound<ConfigResponse>().ToTask<Response>();
        }
    }
}