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

            #region Validate default columns exist

            var defaultColumnsExist =
                tableMetadata.Columns.ContainsKey(SettingProperty.Name) &&
                tableMetadata.Columns.ContainsKey(SettingProperty.Value);

            if (!defaultColumnsExist) throw new ArgumentException($"Table metadata does not contain one or more default columns: [{SettingProperty.Name}, {SettingProperty.Value}].");

            #endregion

            #region Validate custom table columns are constrained by 'where'

            var unconstrainedCustomColumns =
                (from customColumn in tableMetadata.Columns.Select(x => x.Key)
                 where
                     !SettingProperty.Exists(customColumn) &&
                     !@where.ContainsKey(customColumn)
                 select customColumn).ToList();
            if (unconstrainedCustomColumns.Any()) throw new ArgumentException($"One or more custom columns are not constrained: [{string.Join(", ", unconstrainedCustomColumns)}]");

            #endregion

        }

        public SqlServer(string name, string nameOrConnectionString)
            : this(name, nameOrConnectionString, DefaultMetadata, ImmutableDictionary<string, object>.Empty) { }

        public SqlServer(string nameOrConnectionString)
            : this(CreateDefaultName<SqlServer>(), nameOrConnectionString) { }

        public static readonly TableMetadata<SqlDbType> DefaultMetadata =
            TableMetadata<SqlDbType>
                .Create("dbo", "Setting")
                .AddColumn("Name", SqlDbType.NVarChar, 200)
                .AddColumn("Value", SqlDbType.NVarChar, -1);

        protected override ICollection<ISetting> ReadCore(SettingPath settingPath)
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

        protected override int WriteCore(IGrouping<SettingPath, ISetting> settings)
        {
            var affectedRows = 0;

            void DeleteObsoleteSettings(SqlConnection connection, SqlTransaction transaction)
            {
                using (var deleteCommand = _settingCommandFactory.CreateDeleteCommand(connection, settings.Key, _where))
                {
                    deleteCommand.Transaction = transaction;
                    deleteCommand.Prepare();
                    affectedRows += deleteCommand.ExecuteNonQuery();
                }
            }

            void InsertNewSettings(SqlConnection connection, SqlTransaction transaction)
            {
                foreach (var setting in settings)
                {
                    using (var cmd = _settingCommandFactory.CreateInsertCommand(connection, setting.Path, setting.Value, _where))
                    {
                        cmd.Transaction = transaction;
                        cmd.Prepare();
                        affectedRows += cmd.ExecuteNonQuery();
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
            }
            return affectedRows;
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
        public static TableMetadata<SqlDbType> AddNameColumn(this TableMetadata<SqlDbType> tableMetadata, SqlDbType dbType = SqlDbType.NVarChar, int length = 200)
        {
            return tableMetadata.AddColumn(SettingProperty.Name, dbType, length);
        }

        public static TableMetadata<SqlDbType> AddValueColumn(this TableMetadata<SqlDbType> tableMetadata, SqlDbType dbType = SqlDbType.NVarChar, int length = -1)
        {
            return tableMetadata.AddColumn(SettingProperty.Value, dbType, length);
        }
    }
}
