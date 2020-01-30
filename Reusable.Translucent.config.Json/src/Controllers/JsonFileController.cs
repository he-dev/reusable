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

        public JsonFileController(ControllerName controllerName, string basePath, string fileName) : base(controllerName)
        {
            _configuration =
                new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(fileName)
                    .Build();

            Converter = new JsonSettingConverter();
        }

        [ResourceGet]
        public Task<Response> GetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var data = _configuration[settingIdentifier];

            return
                data is {}
                    ? OK<ConfigResponse>(data).ToTask()
                    : NotFound<ConfigResponse>().ToTask();
        }
    }
}