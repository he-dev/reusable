using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Data;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;

namespace Reusable.Octopus.config.Controllers;

[PublicAPI]
public class DatabaseController<TSetting> : ConfigController where TSetting : class, ISetting
{
    public DatabaseController(ISettingRepository<TSetting> settingRepository) => SettingRepository = settingRepository;

    private ISettingRepository<TSetting> SettingRepository { get; }

    public override async Task<Response> ReadAsync(ConfigRequest request)
    {
        if (await SettingRepository.ReadSetting(request.ResourceName.Peek(), request.CancellationToken) is { } value)
        {
            return Success<ConfigResponse>(request.ResourceName, value);
        }
        else
        {
            return NotFound<ConfigResponse>(request.ResourceName);
        }
    }

    public override async Task<Response> CreateAsync(ConfigRequest request)
    {
        if (request.Body.Peek() is string json)
        {
            await SettingRepository.CreateOrUpdateSetting(request.ResourceName.Peek(), json, request.CancellationToken);
            return Success<ConfigResponse>(request.ResourceName);
        }
        else
        {
            throw DynamicException.Create("InvalidRequest", $"Expected string body but got '{request.Body.Peek().GetType().ToPrettyString()}'.");
        }
    }
}