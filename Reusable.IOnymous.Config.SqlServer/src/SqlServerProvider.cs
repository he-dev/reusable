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

        public SqlServerProvider(string nameOrConnectionString) : base(ImmutableContainer.Empty)
        {
            ConnectionString = ConnectionStringRepository.Default.GetConnectionString(nameOrConnectionString);
            TableName = (DefaultSchema, DefaultTable);
            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Get, GetAsync)
                    .Add(RequestMethod.Put, PutAsync);
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

        private async Task<IResource> GetAsync(Request request)
        {
            var settingIdentifier = UriConverter?.Convert<string>(request.Uri) ?? request.Uri;
            //metadata = metadata.SetItem(From<IResourceMeta>.Select(x => x.ActualName), settingIdentifier);

            return await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using (var command = connection.CreateSelectCommand(TableName, settingIdentifier, ColumnMappings, Where, Fallback))
                using (var settingReader = command.ExecuteReader())
                {
                    if (await settingReader.ReadAsync(token))
                    {
                        var value = settingReader[ColumnMappings.MapOrDefault(SqlServerColumn.Value)];
                        value = ValueConverter.Convert(value, request.Properties.GetDataType());
                        return new SqlServerResource(request.Properties.Copy(Resource.PropertySelector), value);
                    }
                    else
                    {
                        return new SqlServerResource(request.Properties.Copy(Resource.PropertySelector));
                    }
                }
            }, CancellationToken.None);
        }

        private async Task<IResource> PutAsync(Request request)
        {
            var settingIdentifier = UriConverter?.Convert<string>(request.Uri) ?? request.Uri;

            using (var body = await request.CreateBodyStreamAsync())
            {
                var value = await ResourceHelper.Deserialize<object>(body, request.Properties);
                value = ValueConverter.Convert(value, typeof(string));

                await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
                {
                    using (var cmd = connection.CreateUpdateCommand(TableName, settingIdentifier, ColumnMappings, Where, value))
                    {
                        await cmd.ExecuteNonQueryAsync(token);
                    }
                }, request.Properties.GetItemOrDefault(Request.PropertySelector.Select(x => x.CancellationToken)));
            }

            return await GetAsync(new Request.Get(request.Uri)
            {
                Properties = request.Properties.CopyResourceProperties()
            });
        }
    }

    internal class SqlServerResource : Resource
    {
        [CanBeNull]
        private readonly object _value;

        internal SqlServerResource(IImmutableContainer properties, object value = default)
            : base(properties
                .SetExists(!(value is null))
                .SetFormat(value is string ? MimeType.Text : MimeType.Binary))
        {
            _value = value;
        }

        public override async Task CopyToAsync(Stream stream)
        {
            if (Properties.GetFormat() == MimeType.Text)
            {
                using (var s = await ResourceHelper.SerializeTextAsync((string)_value))
                {
                    await s.Rewind().CopyToAsync(stream);
                }
            }

            if (Properties.GetFormat() == MimeType.Binary)
            {
                using (var s = await ResourceHelper.SerializeBinaryAsync(_value))
                {
                    await s.Rewind().CopyToAsync(stream);
                }
            }
        }
    }
}