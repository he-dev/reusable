using System;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Data.Repositories;
using Reusable.OneTo1;
using Reusable.Quickey;
using Reusable.Utilities.SqlClient;

namespace Reusable.IOnymous.Config
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
        public async Task<IResource> GetSettingAsync(Request request)
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
                        return new JsonResource
                        (
                            (string)value,
                            request
                                .Context
                                .Copy<ResourceProperties>()
                                .SetItem(SettingProperty.Converter, ResourceConverter)
                        );
                    }
                    else
                    {
                        return DoesNotExist(request);
                    }
                }
            }, request.Context.GetItemOrDefault(RequestProperty.CancellationToken));
        }

        [ResourcePut]
        public async Task<IResource> SetSettingAsync(Request request)
        {
            var settingIdentifier = GetResourceName(request.Uri);
            var value = ResourceConverter.Convert(request.Body, typeof(string));
            await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using (var cmd = connection.CreateUpdateCommand(TableName, settingIdentifier, ColumnMappings, Where, value))
                {
                    await cmd.ExecuteNonQueryAsync(token);
                }
            }, request.Context.GetItemOrDefault(RequestProperty.CancellationToken));

            return await GetSettingAsync(new Request.Get(request.Uri)
            {
                Context = request.Context.Copy<ResourceProperties>()
            });
        }
    }
}