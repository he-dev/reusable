using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Translucent.Data;
using Reusable.Utilities.JsonNet.Converters;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public class SqlServerController<TSetting> : ConfigController where TSetting : class, ISetting
    {
        private readonly ISettingRepository<TSetting> _settingRepository;

        public SqlServerController(ISettingRepository<TSetting> settingRepository)
        {
            _settingRepository = settingRepository;
            Converter = new TypeConverterStack
            {
                new ObjectToJson
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
                },
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

        public SqlServerController(string connectionString, WhereDelegate<TSetting> where)
            : this(new SettingRepository<TSetting>(connectionString) { Where = where }) { }

        public override async Task<Response> ReadAsync(ConfigRequest request)
        {
            if (await _settingRepository.ReadSetting(request.ResourceName, request.CancellationToken) is {} setting)
            {
                var value = Converter.ConvertOrThrow(setting.Value, request.SettingType);
                return Success<ConfigResponse>(request.ResourceName, value);
            }
            else
            {
                return NotFound<ConfigResponse>(request.ResourceName);
            }
        }

        public override async Task<Response> CreateAsync(ConfigRequest request)
        {
            var value = Converter.ConvertOrThrow<string>(request.Body!);
            await _settingRepository.CreateOrUpdateSetting(request.ResourceName, value, request.CancellationToken);
            return Success<ConfigResponse>(request.ResourceName);
        }
    }
}