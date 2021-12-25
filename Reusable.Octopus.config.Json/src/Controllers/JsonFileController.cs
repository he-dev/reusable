using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Data;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers;

/// <summary>
/// This controller reads the new json file from the Microsoft.Extensions.Configuration namespace.
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
    }

    public override Task<Response> ReadAsync(ConfigRequest request)
    {
        var data = _configuration[request.ResourceName.Peek()];

        return
            data is { }
                ? Success<ConfigResponse>(request.ResourceName, data).ToTask()
                : NotFound<ConfigResponse>(request.ResourceName).ToTask();
    }
}