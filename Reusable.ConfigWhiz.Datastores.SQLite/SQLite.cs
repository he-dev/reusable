using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Reusable.ConfigWhiz.Data;
using Reusable.Data;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz.Datastores
{
    public class SQLite : Datastore
    {
        private Encoding _dataEncoding = Encoding.Default;
        private Encoding _settingEncoding = Encoding.UTF8;

        private readonly string _connectionString;
        private readonly SettingCommandFactory _settingCommandFactory;
        private readonly IImmutableDictionary<string, object> _where;

        public SQLite(object handle, string nameOrConnectionString, TableMetadata<DbType> tableMetadata, IImmutableDictionary<string, object> where)
            : base(handle, new[] { typeof(string) })
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

        public static readonly TableMetadata<DbType> DefaultMetadata = TableMetadata<DbType>.Create("dbo", "Setting").AddColumn("Name", DbType.String, 200).AddColumn("Value", DbType.String, -1);

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

        public override Result<IEnumerable<ISetting>> Read(SettingPath settingPath)
        {
            using (var connection = OpenConnection())
            using (var command = _settingCommandFactory.CreateSelectCommand(connection, settingPath, _where))
            {
                command.Prepare();

                using (var settingReader = command.ExecuteReader())
                {
                    var settings = new List<ISetting>();
                    while (settingReader.Read())
                    {
                        var value = (string)settingReader[nameof(Setting.Value)];

                        var setting = new Setting
                        {
                            Path = SettingPath.Parse((string)settingReader[SettingProperty.Name]),
                            Value = RecodeDataEnabled ? value.Recode(DataEncoding, SettingEncoding) : value
                        };
                        settings.Add(setting);
                    }
                    return settings;
                }
            }
        }

        public override Result Write(IGrouping<SettingPath, ISetting> settings)
        {
            void DeleteObsoleteSettings(SQLiteConnection connection, SQLiteTransaction transaction)
            {
                using (var deleteCommand = _settingCommandFactory.CreateDeleteCommand(connection, settings.Key, _where))
                {
                    deleteCommand.Transaction = transaction;
                    deleteCommand.Prepare();
                    deleteCommand.ExecuteNonQuery();
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

                    using (var insertCommand = _settingCommandFactory.CreateInsertCommand(connection, setting.Path, setting.Value, _where))
                    {
                        insertCommand.Transaction = transaction;
                        insertCommand.Prepare();
                        insertCommand.ExecuteNonQuery();
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
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return ex;
                }
                return Result.Ok();
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
