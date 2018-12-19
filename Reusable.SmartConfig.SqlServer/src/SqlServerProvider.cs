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
using Reusable.OneTo1;
using Reusable.SmartConfig.Internal;
using Reusable.Utilities.SqlClient;

namespace Reusable.SmartConfig
{
    using static ResourceMetadataKeys;
    using static SqlServerColumn;

    public class SqlServerProvider : ResourceProvider
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        [CanBeNull] private readonly ITypeConverter _uriStringToSettingIdentifierConverter;

        private SqlFourPartName _tableName;

        public SqlServerProvider
        (
            string nameOrConnectionString,
            ITypeConverter uriStringToSettingIdentifierConverter = null
        )
            : base(
                ResourceMetadata
                    .Empty
                    .Add(CanGet, true)
                    .Add(CanPut, true)
                    .Add(ResourceMetadataKeys.Scheme, "setting")
            )
        {
            _uriStringToSettingIdentifierConverter = uriStringToSettingIdentifierConverter;
            ConnectionString = ConnectionStringRepository.Default.GetConnectionString(nameOrConnectionString);

            TableName = (DefaultSchema, DefaultTable);
        }

        [NotNull]
        public string ConnectionString { get; }

        [NotNull]
        public SqlFourPartName TableName
        {
            get => _tableName;
            set => _tableName = value ?? throw new ArgumentNullException(nameof(TableName));
        }

        [CanBeNull]
        public IImmutableDictionary<SqlServerColumn, ImplicitString> ColumnMappings { get; set; }

        [CanBeNull]
        public IImmutableDictionary<string, object> Where { get; set; }

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            var settingIdentifier = (string)_uriStringToSettingIdentifierConverter?.Convert(uri, typeof(string)) ?? uri;

            return await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using (var command = connection.CreateSelectCommand(TableName, settingIdentifier, ColumnMappings, Where))
                using (var settingReader = command.ExecuteReader())
                {
                    return
                        await settingReader.ReadAsync(token)
                            ? new SqlServerResourceInfo(uri, (string)settingReader[ColumnMappings.MapOrDefault(Value)])
                            : new SqlServerResourceInfo(uri, default);
                }
            }, CancellationToken.None);
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream stream, ResourceMetadata metadata = null)
        {
            var settingIdentifier = (string)_uriStringToSettingIdentifierConverter?.Convert(uri, typeof(string)) ?? uri;

            using (var valueReader = new StreamReader(stream))
            {
                var value = await valueReader.ReadToEndAsync();

                await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
                {
                    using (var cmd = connection.CreateUpdateCommand(TableName, settingIdentifier, ColumnMappings, Where, value))
                    {
                        await cmd.ExecuteNonQueryAsync(token);
                    }
                }, CancellationToken.None);

                return await GetAsync(uri);
            }
        }
    }

    internal class SqlServerResourceInfo : ResourceInfo
    {
        [CanBeNull] private readonly string _value;

        internal SqlServerResourceInfo([NotNull] UriString uri, [CanBeNull] string value) : base(uri)
        {
            _value = value;
        }

        public override bool Exists => !(_value is null);

        public override long? Length => _value?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            // ReSharper disable once AssignNullToNotNullAttribute - this isn't null here
            using (var valueStream = _value.ToStreamReader())
            {
                await valueStream.BaseStream.CopyToAsync(stream);
            }
        }

        protected override Task<object> DeserializeAsyncInternal(Type targetType)
        {
            return Task.FromResult<object>(_value);
        }
    }

    public class SqlServerColumn
    {
        private readonly string _name;

        private SqlServerColumn(string name) => _name = name;

        public static readonly SqlServerColumn Name = new SqlServerColumn(nameof(Name));

        public static readonly SqlServerColumn Value = new SqlServerColumn(nameof(Value));
        
        // todo - for future use
        //public static readonly SqlServerColumn ModifiedOn = new SqlServerColumn(nameof(ModifiedOn));
        //public static readonly SqlServerColumn CreatedOn = new SqlServerColumn(nameof(CreatedOn));

        public static implicit operator string(SqlServerColumn column) => column._name;
    }

    public static class SqlServerColumnMappingExtensions
    {
        [NotNull]
        public static string MapOrDefault(this IImmutableDictionary<SqlServerColumn, ImplicitString> mappings, SqlServerColumn column)
        {
            return
                mappings is null
                    ? column
                    : mappings.TryGetValue(column, out var mapping) && mapping
                        ? (string)mapping
                        : (string)column;
        }
    }
}