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
using Reusable.SmartConfig.Data;
using Reusable.Stratus;
using Reusable.Utilities.SqlClient;

namespace Reusable.SmartConfig
{
    using Internal;
    using static ValueProviderMetadataKeyNames;

    public class SqlServerProvider : ValueProvider
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        private IImmutableDictionary<string, object> _where = ImmutableDictionary<string, object>.Empty;

        private SqlFourPartName _tableName;

        private SqlServerColumnMapping _columnMapping;

        public SqlServerProvider
        (
            string nameOrConnectionString,
            ValueProviderMetadata metadata
        )
            : base(
                metadata
                    .Add(CanDeserialize, true)
                    .Add(CanSerialize, true)
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

        public override async Task<IValueInfo> GetValueInfoAsync(string name, ValueProviderMetadata metadata = null)
        {
            return await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using (var command = connection.CreateSelectCommand(TableName, Where, ColumnMapping, name))
                using (var settingReader = command.ExecuteReader())
                {
                    return
                        await settingReader.ReadAsync(token)
                            ? new SqlServerValueInfo(name, (string)settingReader[ColumnMapping.Value])
                            : new SqlServerValueInfo(name, default);
                }
            }, CancellationToken.None);
        }

        public override async Task<IValueInfo> SerializeAsync(string name, Stream value, ValueProviderMetadata metadata = null)
        {
            using (var valueReader = new StreamReader(value))
            {
                return await SerializeAsync(name, await valueReader.ReadToEndAsync());
            }
        }

        public override async Task<IValueInfo> SerializeAsync(string name, object value, ValueProviderMetadata metadata = null)
        {
            await SqlHelper.ExecuteAsync(ConnectionString, async (connection, token) =>
            {
                using (var cmd = connection.CreateUpdateCommand(TableName, Where, ColumnMapping, name, value))
                {
                    await cmd.ExecuteNonQueryAsync(token);
                }
            }, CancellationToken.None);

            return await GetValueInfoAsync(name);
        }

        public override Task<IValueInfo> DeleteAsync(string name, ValueProviderMetadata metadata = null)
        {
            throw new NotImplementedException();
        }
    }

    internal class SqlServerValueInfo : ValueInfo
    {
        [CanBeNull]
        private readonly string _value;

        internal SqlServerValueInfo([NotNull] string name, [CanBeNull] string value) : base(name)
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


    public class SqlServer : SettingProvider
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        private IReadOnlyDictionary<string, object> _where = new Dictionary<string, object>();

        private SqlFourPartName _settingTableName;

        private SqlServerColumnMapping _columnMapping;

        public SqlServer(string nameOrConnectionString, ISettingConverter converter) : base(new SettingNameFactory(), converter)
        {
            ConnectionString = ConnectionStringRepository.Default.GetConnectionString(nameOrConnectionString);

            SettingTableName = (DefaultSchema, DefaultTable);
            ColumnMapping = new SqlServerColumnMapping();
        }

        [NotNull]
        public string ConnectionString { get; }

        [NotNull]
        public SqlFourPartName SettingTableName
        {
            get => _settingTableName;
            set => _settingTableName = value ?? throw new ArgumentNullException(nameof(SettingTableName));
        }

        [NotNull]
        public SqlServerColumnMapping ColumnMapping
        {
            get => _columnMapping;
            set => _columnMapping = value ?? throw new ArgumentNullException(nameof(ColumnMapping));
        }

        public IReadOnlyDictionary<string, object> Where
        {
            get => _where;
            set => _where = value ?? throw new ArgumentNullException(nameof(Where));
        }

        protected override ISetting Read(SettingName name)
        {
            return SqlHelper.Execute(ConnectionString, connection => Read(connection, name));
        }

        private ISetting Read(SqlConnection connection, SettingName name)
        {
            using (var command = connection.CreateSelectCommand(this, new[] { (SoftString)name }))
            using (var settingReader = command.ExecuteReader())
            {
                if (settingReader.Read())
                {
                    var setting = new Setting(
                        (string)settingReader[ColumnMapping.Name],
                        settingReader[ColumnMapping.Value]
                    );

                    if (settingReader.Read())
                    {
                        //throw CreateAmbiguousSettingException(names);
                    }

                    return setting;
                }

                return null;
            }
        }

        protected override void Write(ISetting setting)
        {
            SqlHelper.Execute(
                ConnectionString, connection =>
                {
                    using (var cmd = connection.CreateUpdateCommand(this, setting))
                    {
                        //cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                }
            );
        }
    }
}