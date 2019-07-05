using System;
using System.Collections.Immutable;
using System.IO;
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
    public class SqlServerProvider : SettingProvider
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        private SqlFourPartName _tableName;

        public SqlServerProvider(string nameOrConnectionString) : base(ImmutableSession.Empty)
        {
            ConnectionString = ConnectionStringRepository.Default.GetConnectionString(nameOrConnectionString);
            TableName = (DefaultSchema, DefaultTable);
        }

        [CanBeNull]
        public ITypeConverter UriConverter { get; set; } = DefaultUriStringConverter;

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
        
        public (string Name, object Value) Fallback { get; set; }

        protected override async Task<IResource> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            var settingIdentifier = UriConverter?.Convert<string>(uri) ?? uri;
            metadata = metadata.SetItem(From<IResourceMeta>.Select(x => x.ActualName), settingIdentifier);

            return await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using (var command = connection.CreateSelectCommand(TableName, settingIdentifier, ColumnMappings, Where, Fallback))
                using (var settingReader = command.ExecuteReader())
                {
                    if (await settingReader.ReadAsync(token))
                    {
                        var value = settingReader[ColumnMappings.MapOrDefault(SqlServerColumn.Value)];
                        value = ValueConverter.Convert(value, metadata.GetItemOrDefault(From<IResourceMeta>.Select(x => x.Type)));
                        return new SqlServerResource(uri, value, metadata);
                    }
                    else
                    {
                        return new SqlServerResource(uri, default, metadata);
                    }
                }
            }, CancellationToken.None);
        }

        protected override async Task<IResource> PutAsyncInternal(UriString uri, Stream stream, IImmutableSession metadata)
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
            }, metadata.GetItemOrDefault(From<IRequestMeta>.Select(x => x.CancellationToken)));

            return await GetAsync(uri, metadata);
        }
    }

    internal class SqlServerResource : Resource
    {
        [CanBeNull] private readonly object _value;

        internal SqlServerResource([NotNull] UriString uri, [CanBeNull] object value, IImmutableSession metadata)
            : base(uri, metadata.SetItem(From<IResourceMeta>.Select(x => x.Format), value is string ? MimeType.Text : MimeType.Binary))
        {
            _value = value;
        }

        public override bool Exists => !(_value is null);

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            var format = Metadata.GetItemOrDefault(From<IResourceMeta>.Select(x => x.Format));
            if (format == MimeType.Text)
            {
                using (var s = await ResourceHelper.SerializeTextAsync((string)_value))
                {
                    await s.Rewind().CopyToAsync(stream);
                }
            }

            if (format == MimeType.Binary)
            {
                using (var s = await ResourceHelper.SerializeBinaryAsync(_value))
                {
                    await s.Rewind().CopyToAsync(stream);
                }
            }
        }
    }
}