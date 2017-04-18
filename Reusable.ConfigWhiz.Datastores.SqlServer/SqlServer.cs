using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Reusable.ConfigWhiz.Data;
using Reusable.Data;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz.Datastores
{
    public class SqlServer : Datastore
    {
        private readonly string _connectionString;
        private readonly SettingCommandFactory _settingCommandFactory;
        private readonly IImmutableDictionary<string, object> _where;

        public SqlServer(string name, string nameOrConnectionString, TableMetadata<SqlDbType> tableMetadata, IImmutableDictionary<string, object> where)
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

        public SqlServer(string name, string nameOrConnectionString)
            : this(name, nameOrConnectionString, DefaultMetadata, ImmutableDictionary<string, object>.Empty) { }

        public SqlServer(string nameOrConnectionString)
            : this(CreateDefaultName<SqlServer>(), nameOrConnectionString) { }

        public static readonly TableMetadata<SqlDbType> DefaultMetadata = TableMetadata<SqlDbType>.Create("dbo", "Setting").AddColumn("Name", SqlDbType.NVarChar, 200).AddColumn("Value", SqlDbType.NVarChar, -1);

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
                        var result = new Setting
                        {
                            Path = SettingPath.Parse((string)settingReader[SettingProperty.Name]),
                            Value = settingReader[SettingProperty.Value],
                        };
                        settings.Add(result);
                    }
                    return settings;
                }
            }
        }

        public override Result Write(IGrouping<SettingPath, ISetting> settings)
        {
            void DeleteObsoleteSettings(SqlConnection connection, SqlTransaction transaction)
            {
                using (var deleteCommand = _settingCommandFactory.CreateDeleteCommand(connection, settings.Key, _where))
                {
                    deleteCommand.Transaction = transaction;
                    deleteCommand.Prepare();
                    deleteCommand.ExecuteNonQuery();
                }
            }

            void InsertNewSettings(SqlConnection connection, SqlTransaction transaction)
            {
                foreach (var setting in settings)
                {
                    using (var insertCommand = _settingCommandFactory.CreateInsertCommand(connection, setting.Path, setting.Value, _where))
                    {
                        insertCommand.Transaction = transaction;
                        insertCommand.Prepare();
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }

            var sw = Stopwatch.StartNew();

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
                    return Result.Fail(ex, sw.Elapsed);
                }
            }

            return Result.Ok(sw.Elapsed);
        }

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }

    public class ColumnConfigurationNotFoundException : Exception
    {
        internal ColumnConfigurationNotFoundException(string column)
        {
            Column = column;
        }

        public string Column { get; set; }

        public override string Message => $"\"{Column}\" column configuration not found. Ensure that it is set via the \"{nameof(SqlServer)}\" builder.";
    }

    public static class TableMetadataExtensions
    {
        public static TableMetadata<SqlDbType> AddNameColumn(this TableMetadata<SqlDbType> tableMetadata, int length = 200)
        {
            return tableMetadata.AddColumn("Name", SqlDbType.NVarChar, length);
        }

        public static TableMetadata<SqlDbType> AddValueColumn(this TableMetadata<SqlDbType> tableMetadata, int length = -1)
        {
            return tableMetadata.AddColumn("Value", SqlDbType.NVarChar, length);
        }
    }
}
