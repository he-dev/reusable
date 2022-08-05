using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Reusable.Marbles.Extensions;
using Reusable.Synergy.Requests;

namespace Reusable.Synergy.Controllers;

public abstract class JsonConfigService<T> : ConfigService<T> where T : IRequest
{
    protected IConfiguration Configuration { get; init; } = null!;
}

public static class JsonConfigService
{
    public class Read : JsonConfigService<IReadSetting>
    {
        public Read(IConfiguration configuration) => Configuration = configuration;

        public Read(string basePath, string fileName)
        {
            Configuration =
                new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(fileName)
                    .Build();
        }

        protected override Task<object> InvokeAsync(IReadSetting setting)
        {
            var data = Configuration[setting.Name];

            return
                data is { }
                    ? data.ToTask<object>()
                    : Unit.Default.ToTask<object>();
        }
    }
}