using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Synergy.Requests;

namespace Reusable.Synergy.Controllers;

public abstract class DbService<TRequest, TSetting> : ConfigService<TRequest>
    where TRequest : IRequest
    where TSetting : ISetting
{
    protected DbService(ISettingRepository<TSetting> settingRepository) => SettingRepository = settingRepository;

    protected ISettingRepository<TSetting> SettingRepository { get; }
}

public static class DbService
{
    public class Read<T> : DbService<IReadSetting, T> where T : ISetting
    {
        public Read(ISettingRepository<T> settingRepository) : base(settingRepository) { }

        protected override async Task<object> InvokeAsync(IReadSetting request)
        {
            return
                await SettingRepository.ReadSetting(request.Name, request.CancellationToken) is { } value
                    ? value
                    : Unit.Default;
        }
    }

    public class Write<T> : DbService<IWriteSetting, T> where T : ISetting
    {
        public Write(ISettingRepository<T> settingRepository) : base(settingRepository) { }

        protected override async Task<object> InvokeAsync(IWriteSetting setting)
        {
            if (setting.Value is string value)
            {
                await SettingRepository.CreateOrUpdateSetting(setting.Name, value, setting.CancellationToken);
            }

            return Unit.Default;
        }
    }
}