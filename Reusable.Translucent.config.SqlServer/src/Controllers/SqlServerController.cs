using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json.Converters;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Data;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.SqlClient;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public class SqlServerController : ConfigController
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        public SqlServerController(string connectionString)
        {
            ConnectionString = connectionString;
            //Converter = new JsonSettingConverter();
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

        public string ConnectionString { get; }

        public SqlFourPartName TableName { get; set; } = (DefaultSchema, DefaultTable);

        public IImmutableDictionary<SqlServerColumn, SoftString> ColumnMappings { get; set; } = ImmutableDictionary<SqlServerColumn, SoftString>.Empty;

        public IImmutableDictionary<string, object> Where { get; set; } = ImmutableDictionary<string, object>.Empty;

        public IImmutableDictionary<string, object> Fallback { get; set; } = ImmutableDictionary<string, object>.Empty;

        public override async Task<Response> ReadAsync(ConfigRequest request)
        {
            return await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using var command = connection.CreateSelectCommand(TableName, request.ResourceName, ColumnMappings, Where, Fallback);
                using var settingReader = command.ExecuteReader();

                if (await settingReader.ReadAsync(token))
                {
                    var value = settingReader[ColumnMappings.MapOrDefault(SqlServerColumn.Value)];
                    value = Converter.ConvertOrDefault(value, request.SettingType);
                    return Success<ConfigResponse>(request.ResourceName, value);
                }
                else
                {
                    return NotFound<ConfigResponse>(request.ResourceName);
                }
            }, request.CancellationToken);
        }

        public override async Task<Response> CreateAsync(ConfigRequest request)
        {
            var value = Converter.ConvertOrDefault(request.Body, typeof(string));

            await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using var cmd = connection.CreateUpdateCommand(TableName, request.ResourceName, ColumnMappings, Where, value);
                await cmd.ExecuteNonQueryAsync(token);
            }, request.CancellationToken);

            return Success<ConfigResponse>(request.ResourceName);
        }
    }
}