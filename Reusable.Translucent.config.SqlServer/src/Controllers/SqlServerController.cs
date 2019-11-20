using System.Collections.Immutable;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.OneTo1;
using Reusable.Quickey;
using Reusable.Translucent.Converters;
using Reusable.Utilities.SqlClient;

namespace Reusable.Translucent.Controllers
{
    public class SqlServerController : ConfigController
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        public SqlServerController(string connectionString, IImmutableContainer? properties = default)
            : base(properties
                .ThisOrEmpty()
                .SetItem(ConnectionString, connectionString)
                .SetItemWhenNotExists(Setting.Converter, new JsonSettingConverter())
                .SetItemWhenNotExists(TableName, (DefaultSchema, DefaultTable))
            ) { }

        [ResourceGet]
        public async Task<Response> GetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);

            var connectionString = Properties.GetItem(ConnectionString);
            return await SqlHelper.ExecuteAsync(connectionString, async (connection, token) =>
            {
                using var command = connection.CreateSelectCommand
                (
                    Properties.GetItem(TableName),
                    settingIdentifier,
                    Properties.GetItemOrDefault(ColumnMappings),
                    Properties.GetItemOrDefault(Where),
                    Properties.GetItemOrDefault(Fallback)
                );
                using var settingReader = command.ExecuteReader();

                if (await settingReader.ReadAsync(token))
                {
                    var value = settingReader[Properties.GetItem(ColumnMappings).MapOrDefault(SqlServerColumn.Value)];
                    value = Properties.GetItem(Setting.Converter).Convert(value, request.Metadata.GetItem(Resource.Type));
                    return OK(request, value, settingIdentifier);
                }
                else
                {
                    return NotFound();
                }
            }, request.Metadata.GetItemOrDefault(Request.CancellationToken));
        }

        [ResourcePut]
        public async Task<Response> SetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var value = Properties.GetItem(Setting.Converter).Convert(request.Body, typeof(string));

            var connectionString = Properties.GetItem(ConnectionString);
            await SqlHelper.ExecuteAsync(connectionString, async (connection, token) =>
            {
                using var cmd = connection.CreateUpdateCommand
                (
                    Properties.GetItem(TableName),
                    settingIdentifier,
                    Properties.GetItemOrDefault(ColumnMappings),
                    Properties.GetItemOrDefault(Where),
                    value
                );
                await cmd.ExecuteNonQueryAsync(token);
            }, request.Metadata.GetItemOrDefault(Request.CancellationToken));

            return OK();
        }

        #region Properties

        private static readonly From<SqlServerController>? This;

        public static Selector<string> ConnectionString { get; } = This.Select(() => ConnectionString);
        public static Selector<SqlFourPartName> TableName { get; } = This.Select(() => TableName);
        public static Selector<IImmutableDictionary<SqlServerColumn, SoftString>> ColumnMappings { get; } = This.Select(() => ColumnMappings);
        public static Selector<IImmutableDictionary<string, object>> Where { get; } = This.Select(() => Where);
        public static Selector<IImmutableDictionary<string, object>> Fallback { get; } = This.Select(() => Fallback);

        #endregion
    }
}