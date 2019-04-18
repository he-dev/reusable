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
using Reusable.OneTo1.Converters;
using Reusable.Utilities.SqlClient;

namespace Reusable.SmartConfig
{
    using static SqlServerColumn;

    public class SqlServerProvider : SettingProvider
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        private SqlFourPartName _tableName;

        public SqlServerProvider(string nameOrConnectionString) : base(Metadata.Empty)
        {
            ConnectionString = ConnectionStringRepository.Default.GetConnectionString(nameOrConnectionString);
            TableName = (DefaultSchema, DefaultTable);
        }

        [CanBeNull]
        public ITypeConverter UriConverter { get; set; } = new UriStringToSettingIdentifierConverter();

        public ITypeConverter ValueConverter { get; set; } = new JsonSettingConverter();

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

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, Metadata metadata)
        {
            var settingIdentifier = UriConverter?.Convert<string>(uri) ?? uri;
            metadata = metadata.Union(Metadata.Empty.Resource(s => s.InternalName(settingIdentifier)));

            return await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using (var command = connection.CreateSelectCommand(TableName, settingIdentifier, ColumnMappings, Where))
                using (var settingReader = command.ExecuteReader())
                {
                    if (await settingReader.ReadAsync(token))
                    {
                        var value = settingReader[ColumnMappings.MapOrDefault(Value)];
                        value = ValueConverter.Convert(value, metadata.Resource().Type());
                        return new SqlServerResourceInfo(uri, value, metadata);
                    }
                    else
                    {
                        return new SqlServerResourceInfo(uri, default, metadata);
                    }
                }
            }, CancellationToken.None);
        }

        protected override async Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream stream, Metadata metadata)
        {
            var settingIdentifier = UriConverter?.Convert<string>(uri) ?? uri;

            var value = await ResourceHelper.Deserialize<object>(stream, metadata);
            value = ValueConverter.Convert(value, typeof(string));

            await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using (var cmd = connection.CreateUpdateCommand(TableName, settingIdentifier, ColumnMappings, Where, value))
                {
                    await cmd.ExecuteNonQueryAsync(token);
                }
            }, metadata.CancellationToken());

            return await GetAsync(uri, metadata);
        }
    }

    internal class SqlServerResourceInfo : ResourceInfo
    {
        [CanBeNull] private readonly object _value;

        internal SqlServerResourceInfo([NotNull] UriString uri, [CanBeNull] object value, Metadata metadata)
            : base(uri, m => m.Format(value is string ? MimeType.Text : MimeType.Binary).Union(metadata))
        {
            _value = value;
        }

        public override bool Exists => !(_value is null);

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            if (Metadata.Resource().Format() == MimeType.Text)
            {
                using (var s = await ResourceHelper.SerializeTextAsync((string)_value))
                {
                    await s.Rewind().CopyToAsync(stream);
                }
            }

            if (Metadata.Resource().Format() == MimeType.Binary)
            {
                using (var s = await ResourceHelper.SerializeBinaryAsync(_value))
                {
                    await s.Rewind().CopyToAsync(stream);
                }
            }
        }
    }
}