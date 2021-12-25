using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Data;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers;

[PublicAPI]
public class SqlServerController<TSetting> : ConfigController where TSetting : class, ISetting
{
    public SqlServerController(ISettingRepository<TSetting> settingRepository)
    {
        SettingRepository = settingRepository;
    }

    public SqlServerController(string connectionString, WhereDelegate<TSetting>? where = default)
        : this(new SettingRepository<TSetting>(connectionString).Also(r =>
        {
            if (where is { })
            {
                r.Where = where;
            }
        })) { }

    private ISettingRepository<TSetting> SettingRepository { get; }

    public override async Task<Response> ReadAsync(ConfigRequest request)
    {
        if (await SettingRepository.ReadSetting(request.ResourceName.Peek(), request.CancellationToken) is { } value)
        {
            //var value = Converter.ConvertOrThrow(setting.Value, request.SettingType);
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
            //var value = Converter.ConvertOrThrow<string>(request.Body!);
            await SettingRepository.CreateOrUpdateSetting(request.ResourceName.Peek(), json, request.CancellationToken);
            return Success<ConfigResponse>(request.ResourceName);
        }
        else
        {
            throw DynamicException.Create("InvalidRequest", $"Expected string body but got '{request.Body.Peek().GetType().ToPrettyString()}'.");
        }
    }
}