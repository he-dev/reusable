using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Data.Repositories;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Translucent.Converters;
using Reusable.Utilities.SqlClient;

namespace Reusable.Translucent.Controllers
{
    public class SqlServerController : SettingController
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        private SqlFourPartName _tableName;

        public SqlServerController(string nameOrConnectionString) : base(ImmutableContainer.Empty)
        {
            ConnectionString = ConnectionStringRepository.Default.GetConnectionString(nameOrConnectionString);
            TableName = (DefaultSchema, DefaultTable);
        }

        public ITypeConverter ResourceConverter { get; set; } = new JsonSettingConverter();

        [NotNull]
        public string ConnectionString { get; }

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

            return await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using (var command = connection.CreateSelectCommand(TableName, settingIdentifier, ColumnMappings, Where, Fallback))
                using (var settingReader = command.ExecuteReader())
                {
                    if (await settingReader.ReadAsync(token))
                    {
                        var value = settingReader[ColumnMappings.MapOrDefault(SqlServerColumn.Value)];
                        return new Response.OK
                        {
                            Body = value,
                            //ContentType = MimeType.Json,
                            Metadata =
                                request
                                    .Metadata
                                    .Copy<ResourceProperties>()
                                    .SetItem(SettingControllerProperties.Converter, ResourceConverter)
                        };
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
            var value = ResourceConverter.Convert(request.Body, typeof(string));
            await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
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
    }
}