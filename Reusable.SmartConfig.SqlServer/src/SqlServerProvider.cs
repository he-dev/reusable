using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data.Repositories;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.IOnymous;
using Reusable.SmartConfig.Data;
using Reusable.Utilities.SqlClient;

namespace Reusable.SmartConfig
{
    using static ResourceMetadataKeys;

    public class SqlServerProvider : ResourceProvider
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        private IImmutableDictionary<string, object> _where = ImmutableDictionary<string, object>.Empty;

        private SqlFourPartName _tableName;

        private SqlServerColumnMapping _columnMapping;

        public SqlServerProvider
        (
            string nameOrConnectionString,
            ResourceMetadata metadata
        )
            : base(
                metadata
                    .Add(CanGet, true)
                    .Add(CanPut, true)
                )
        {
            ConnectionString = ConnectionStringRepository.Default.GetConnectionString(nameOrConnectionString);

            TableName = (DefaultSchema, DefaultTable);
            ColumnMapping = new SqlServerColumnMapping();
        }

        [NotNull]
        public string ConnectionString { get; }

        [NotNull]
        public SqlFourPartName TableName
        {
            get => _tableName;
            set => _tableName = value ?? throw new ArgumentNullException(nameof(TableName));
        }

        [NotNull]
        public SqlServerColumnMapping ColumnMapping
        {
            get => _columnMapping;
            set => _columnMapping = value ?? throw new ArgumentNullException(nameof(ColumnMapping));
        }

        public IImmutableDictionary<string, object> Where
        {
            get => _where;
            set => _where = value ?? throw new ArgumentNullException(nameof(Where));
        }

        public override async Task<IResourceInfo> GetAsync(UriString uri, ResourceMetadata metadata = null)
        {
            var settingName = new SettingName(uri);

            return await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using (var command = connection.CreateSelectCommand(TableName, Where, ColumnMapping, settingName))
                using (var settingReader = command.ExecuteReader())
                {
                    return
                        await settingReader.ReadAsync(token)
                            ? new SqlServerResourceInfo(uri, (string)settingReader[ColumnMapping.Value])
                            : new SqlServerResourceInfo(uri, default);
                }
            }, CancellationToken.None);
        }

        public override async Task<IResourceInfo> PutAsync(UriString uri, Stream stream, ResourceMetadata metadata = null)
        {
            using (var valueReader = new StreamReader(stream))
            {
                var value = await valueReader.ReadToEndAsync();
                var settingName = new SettingName(uri);

                await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
                {
                    using (var cmd = connection.CreateUpdateCommand(TableName, Where, ColumnMapping, settingName, value))
                    {
                        await cmd.ExecuteNonQueryAsync(token);
                    }
                }, CancellationToken.None);

                return await GetAsync(uri);
            }
        }

        public override Task<IResourceInfo> DeleteAsync(UriString uri, ResourceMetadata metadata = null)
        {
            throw new NotImplementedException();
        }
    }

    internal class SqlServerResourceInfo : ResourceInfo
    {
        [CanBeNull]
        private readonly string _value;

        internal SqlServerResourceInfo([NotNull] UriString uri, [CanBeNull] string value) : base(uri)
        {
            _value = value;
        }

        public override bool Exists => !(_value is null);

        public override long? Length => _value?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        public override async Task CopyToAsync(Stream stream)
        {
            if (Exists)
            {
                // ReSharper disable once AssignNullToNotNullAttribute - this isn't null here
                using (var valueStream = _value.ToStreamReader())
                {
                    await valueStream.BaseStream.CopyToAsync(stream);
                }
            }
        }

        public override Task<object> DeserializeAsync(Type targetType)
        {
            return Task.FromResult<object>(_value);
        }
    }    
}