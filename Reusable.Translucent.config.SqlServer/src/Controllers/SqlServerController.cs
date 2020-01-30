using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OneTo1;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Converters;
using Reusable.Translucent.Data;
using Reusable.Utilities.SqlClient;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public class SqlServerController : ConfigController
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        public SqlServerController(ControllerName controllerName, string connectionString) : base(controllerName)
        {
            ConnectionString = connectionString;
            Converter = new JsonSettingConverter();
        }

        public string ConnectionString { get; }

        public SqlFourPartName TableName { get; set; } = (DefaultSchema, DefaultTable);

        public IImmutableDictionary<SqlServerColumn, SoftString> ColumnMappings { get; set; } = ImmutableDictionary<SqlServerColumn, SoftString>.Empty;

        public IImmutableDictionary<string, object> Where { get; set; } = ImmutableDictionary<string, object>.Empty;

        public IImmutableDictionary<string, object> Fallback { get; set; } = ImmutableDictionary<string, object>.Empty;

        [ResourceGet]
        public async Task<Response> GetSettingAsync(Request request)
        {
            var configRequest = (ConfigRequest)request;

            var settingIdentifier = GetResourceName(request.Uri);

            return await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using var command = connection.CreateSelectCommand(TableName, settingIdentifier, ColumnMappings, Where, Fallback);
                using var settingReader = command.ExecuteReader();

                if (await settingReader.ReadAsync(token))
                {
                    var value = settingReader[ColumnMappings.MapOrDefault(SqlServerColumn.Value)];
                    value = Converter.Convert(value, configRequest.SettingType);
                    return OK<ConfigResponse>(value);
                }
                else
                {
                    return NotFound<ConfigResponse>();
                }
            }, request.CancellationToken);
        }

        [ResourcePut]
        public async Task<Response> SetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var value = Converter.Convert(request.Body, typeof(string));

            await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using var cmd = connection.CreateUpdateCommand(TableName, settingIdentifier, ColumnMappings, Where, value);
                await cmd.ExecuteNonQueryAsync(token);
            }, request.CancellationToken);

            return OK<ConfigResponse>();
        }
    }
}