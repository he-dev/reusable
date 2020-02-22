using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Converters;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Translucent.Data;
using Reusable.Utilities.JsonNet.Converters;

namespace Reusable.Translucent.Controllers
{
    /// <summary>
    /// This controller read the new json file from the Microsoft.Extensions.Configuration namespace.
    /// </summary>
    public class JsonFileController : ConfigController
    {
        private readonly IConfiguration _configuration;

        public JsonFileController(string basePath, string fileName)
        {
            _configuration =
                new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(fileName)
                    .Build();

            Converter = new TypeConverterStack
            {
                new JsonToObject
                {
                    Settings =
                    {
                        Converters =
                        {
                            new StringEnumConverter(),
                            new ColorConverter(),
                            new SoftStringConverter()
                        }
                    }
                }
            };
        }

        public override Task<Response> ReadAsync(ConfigRequest request)
        {
            var data = _configuration[request.ResourceName];

            return
                data is {}
                    ? Success<ConfigResponse>(request.ResourceName, data).ToTask<Response>()
                    : NotFound<ConfigResponse>(request.ResourceName).ToTask<Response>();
        }
    }
}