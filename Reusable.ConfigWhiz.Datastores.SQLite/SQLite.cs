using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Paths;
using Reusable.Data;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz.Datastores
{
    // ReSharper disable once InconsistentNaming
    public class SQLite : Datastore
    {
        private Encoding _dataEncoding = Encoding.Default;
        private Encoding _settingEncoding = Encoding.UTF8;

        private readonly string _connectionString;
        private readonly SettingCommandFactory _settingCommandFactory;
        private readonly IImmutableDictionary<string, object> _where;

        public SQLite(string name, string nameOrConnectionString, TableMetadata<DbType> tableMetadata, IImmutableDictionary<string, object> where)
            : base(name, new[] { typeof(string) })
        {
            var connectionStringName = nameOrConnectionString.ExtractConnectionStringName();
            _connectionString =
                connectionStringName.IsNullOrEmpty()
                    ? nameOrConnectionString
                    : AppConfigRepository.Deafult.GetConnectionString(connectionStringName);

            if (_connectionString.IsNullOrEmpty()) throw new ArgumentNullException(nameof(nameOrConnectionString));

            _where = where ?? throw new ArgumentNullException(nameof(where));
            _settingCommandFactory = new SettingCommandFactory(tableMetadata ?? throw new ArgumentNullException(nameof(tableMetadata)));
        }

        public SQLite(string nameOrConnectionString, TableMetadata<DbType> tableMetadata, IImmutableDictionary<string, object> where)
            : this(CreateDefaultName<SQLite>(), nameOrConnectionString, tableMetadata, where) { }

        public static readonly TableMetadata<DbType> DefaultMetadata = 
            TableMetadata<DbType>
                .Create("dbo", "Setting")
                .AddColumn("Name", DbType.String, 200)
                .AddColumn("Value", DbType.String, -1);

        public bool RecodeDataEnabled { get; set; } = true;

        public bool RecodeSettingEnabled { get; set; } = true;

        public Encoding DataEncoding
        {
            get => _dataEncoding;
            set => _dataEncoding = value ?? throw new ArgumentNullException(nameof(DataEncoding));
        }

        public Encoding SettingEncoding
        {
            get => _settingEncoding;
            set => _settingEncoding = value ?? throw new ArgumentNullException(nameof(SettingEncoding));
        }

        protected override ICollection<IEntity> ReadCore(SettingIdentifier settingIdentifier)
        {
            using (var connection = OpenConnection())
            using (var command = _settingCommandFactory.CreateSelectCommand(connection, settingIdentifier, _where))
            {
                command.Prepare();

                using (var settingReader = command.ExecuteReader())
                {
                    var settings = new List<IEntity>();
                    while (settingReader.Read())
                    {
                        var value = (string)settingReader[nameof(Entity.Value)];

                        var setting = new Entity
                        {
                            Identifier = SettingIdentifier.Parse((string)settingReader[SettingProperty.Name]),
                            Value = RecodeDataEnabled ? value.Recode(DataEncoding, SettingEncoding) : value
                        };
                        settings.Add(setting);
                    }
                    return settings;
                }
            }
        }

        protected override int WriteCore(IGrouping<SettingIdentifier, IEntity> settings)
        {
            var rowsAffected = 0;

            void DeleteObsoleteSettings(SQLiteConnection connection, SQLiteTransaction transaction)
            {
                using (var deleteCommand = _settingCommandFactory.CreateDeleteCommand(connection, settings.Key, _where))
                {
                    deleteCommand.Transaction = transaction;
                    deleteCommand.Prepare();
                    rowsAffected += deleteCommand.ExecuteNonQuery();
                }
            }

            void InsertNewSettings(SQLiteConnection connection, SQLiteTransaction transaction)
            {
                foreach (var setting in settings)
                {
                    if (RecodeSettingEnabled && setting.Value is string)
                    {
                        setting.Value = ((string)setting.Value).Recode(SettingEncoding, DataEncoding);
                    }

                    using (var insertCommand = _settingCommandFactory.CreateInsertCommand(connection, setting.Identifier, setting.Value, _where))
                    {
                        insertCommand.Transaction = transaction;
                        insertCommand.Prepare();
                        rowsAffected += insertCommand.ExecuteNonQuery();
                    }
                }
            }

            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    DeleteObsoleteSettings(connection, transaction);
                    InsertNewSettings(connection, transaction);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                return rowsAffected;
            }
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }

    public static class StringEncoder
    {
        public static string Recode(this string value, Encoding from, Encoding to)
        {
            return to.GetString(from.GetBytes(value));
        }
    }
}
