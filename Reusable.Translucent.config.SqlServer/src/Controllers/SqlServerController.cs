using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Data.Repositories;
using Reusable.Extensions;
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

        private SqlFourPartName _tableName;

        public SqlServerController(IImmutableContainer properties) : base(properties)
        {
            TableName = (DefaultSchema, DefaultTable);
        }

        public SqlServerController(string connectionString)
            : base(ImmutableContainer.Empty.SetItem(ConnectionString, connectionString).SetItem(Converter, new JsonSettingConverter()))
        {
            //ConnectionString = ConnectionStringRepository.Default.GetConnectionString(nameOrConnectionString);
        }

        [NotNull]
        public SqlFourPartName TableName
        {
            get => _tableName;
            set => _tableName = value ?? throw new ArgumentNullException(nameof(TableName));
        }

        [CanBeNull]

        public IImmutableDictionary<SqlServerColumn, SoftString> ColumnMappings { get; set; }

        [CanBeNull]
        public IImmutableDictionary<string, object> Where { get; set; }

        public (SoftString Name, object Value) Fallback { get; set; }

        [ResourceGet]
        public async Task<Response> GetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);

            var connectionString = Properties.GetItem(ConnectionString);
            return await SqlHelper.ExecuteAsync(connectionString, async (connection, token) =>
            {
                using (var command = connection.CreateSelectCommand(TableName, settingIdentifier, ColumnMappings, Where, Fallback))
                using (var settingReader = command.ExecuteReader())
                {
                    if (await settingReader.ReadAsync(token))
                    {
                        var value = settingReader[ColumnMappings.MapOrDefault(SqlServerColumn.Value)];
                        return OK(request, (string)value, settingIdentifier);
                    }
                    else
                    {
                        return (Response)new Response.NotFound();
                    }
                }
            }, request.Metadata.GetItemOrDefault(Request.CancellationToken));
        }

        [ResourcePut]
        public async Task<Response> SetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var value = Properties.GetItem(Converter).Convert(request.Body, typeof(string));
            
            var connectionString = Properties.GetItem(ConnectionString);
            await SqlHelper.ExecuteAsync(connectionString, async (connection, token) =>
            {
                using (var cmd = connection.CreateUpdateCommand(TableName, settingIdentifier, ColumnMappings, Where, value))
                {
                    await cmd.ExecuteNonQueryAsync(token);
                }
            }, request.Metadata.GetItemOrDefault(Request.CancellationToken));

            //            return await GetSettingAsync(new Request.Get(request.Uri)
            //            {
            //                Metadata = request.Metadata.Copy<IOnymous.ResourceProperties>()
            //            });

            return new Response.OK();
        }

        #region Properties

        private static readonly From<SqlServerController> This;

        public static Selector<string> ConnectionString { get; } = This.Select(() => ConnectionString);

        #endregion
    }
}