using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        public SqlServer(string nameOrConnectionString, TableMetadata<SqlDbType> tableMetadata, IImmutableDictionary<string, object> where)
            : base(new[] { typeof(string) })
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

        public SqlServer(string nameOrConnectionString) : this(nameOrConnectionString, DefaultMetadata, ImmutableDictionary<string, object>.Empty) { }

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
                            Path = SettingPath.Parse((string)settingReader[nameof(Setting.Path)]),
                            Value = settingReader[nameof(Setting.Value)],
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

    internal class SettingCommandFactory
    {
        private readonly TableMetadata<SqlDbType> _tableMetadata;

        public SettingCommandFactory(TableMetadata<SqlDbType> tableMetadata)
        {
            _tableMetadata = tableMetadata;
        }

        public SqlCommand CreateSelectCommand(SqlConnection connection, SettingPath settingPath, IImmutableDictionary<string, object> where)
        {
            var sql = new StringBuilder();

            var dbProviderFactory = DbProviderFactories.GetFactory(connection);
            using (var commandBuilder = dbProviderFactory.CreateCommandBuilder())
            {
                string Sanitize(string identifier) => commandBuilder.QuoteIdentifier(identifier);

                var table = $"{Sanitize(_tableMetadata.SchemaName)}.{Sanitize(_tableMetadata.TableName)}";

                sql.Append($"SELECT *").AppendLine();
                sql.Append($"FROM {table}").AppendLine();
                sql.Append(where.Aggregate(
                    $"WHERE ([{nameof(Setting.Path)}] = @{nameof(Setting.Path)} OR [{nameof(Setting.Path)}] LIKE @{nameof(Setting.Path)} + N'[[]%]')",
                    (result, next) => $"{result} AND {Sanitize(next.Key)} = @{next.Key}")
                );
            }

            var command = connection.CreateCommand();
            command.CommandText = sql.ToString();

            // --- add parameters & values

            AddParameter(command, nameof(Setting.Path), settingPath.ToString("a.b", SettingPathFormatter.Instance));
            AddParameters(command, where);

            return command;
        }

        public SqlCommand CreateDeleteCommand(SqlConnection connection, SettingPath settingPath, IImmutableDictionary<string, object> where)
        {
            /*
             
            DELETE FROM [dbo].[Setting] WHERE [Name] LIKE 'baz%' AND [Environment] = 'boz'

            */

            var sql = new StringBuilder();

            var dbProviderFactory = DbProviderFactories.GetFactory(connection);
            using (var commandBuilder = dbProviderFactory.CreateCommandBuilder())
            {
                string Sanitize(string identifier) => commandBuilder.QuoteIdentifier(identifier);

                var table = $"{Sanitize(_tableMetadata.SchemaName)}.{Sanitize(_tableMetadata.TableName)}";

                sql.Append($"DELETE FROM {table}").AppendLine();
                sql.Append(where.Keys.Aggregate(
                    $"WHERE ([{nameof(Setting.Path)}] = @{nameof(Setting.Path)} OR [{nameof(Setting.Path)}] LIKE @{nameof(Setting.Path)} + N'[[]%]')",
                    (result, next) => $"{result} AND {Sanitize(next)} = @{next} ")
                );
            }

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters & values

            AddParameter(command, nameof(Setting.Path), settingPath.ToString("a.b", SettingPathFormatter.Instance));
            AddParameters(command, where);

            return command;
        }

        public SqlCommand CreateInsertCommand(SqlConnection connection, SettingPath settingPath, object value, IImmutableDictionary<string, object> where)
        {
            /*
             
            UPDATE [Setting]
	            SET [Value] = 'Hallo update!'
	            WHERE [Name]='baz' AND [Environment] = 'boz'
            IF @@ROWCOUNT = 0 
	            INSERT INTO [Setting]([Name], [Value], [Environment])
	            VALUES ('baz', 'Hallo insert!', 'boz')
            
            */

            var sql = new StringBuilder();

            var dbProviderFactory = DbProviderFactories.GetFactory(connection);
            using (var commandBuilder = dbProviderFactory.CreateCommandBuilder())
            {
                string Sanitize(string identifier) => commandBuilder.QuoteIdentifier(identifier);

                var table = $"{Sanitize(_tableMetadata.SchemaName)}.{Sanitize(_tableMetadata.TableName)}";

                sql.Append($"UPDATE {table}").AppendLine();
                sql.Append($"SET [{nameof(Setting.Value)}] = @{nameof(Setting.Value)}").AppendLine();

                sql.Append(where.Keys.Aggregate(
                    $"WHERE ([{nameof(Setting.Path)}] = @{nameof(Setting.Path)} OR [{nameof(Setting.Path)}] LIKE @{nameof(Setting.Path)} + N'[[]%]')",
                    (result, next) => $"{result} AND {Sanitize(next)} = @{next} ")
                ).AppendLine();

                sql.Append($"IF @@ROWCOUNT = 0").AppendLine();

                var columns = where.Keys.Select(Sanitize).Aggregate(
                    $"[{nameof(Setting.Path)}], [{nameof(Setting.Value)}]",
                    (result, next) => $"{result}, {next}"
                );

                sql.Append($"INSERT INTO {table}({columns})").AppendLine();

                var parameterNames = where.Keys.Aggregate(
                    $"@{nameof(Setting.Path)}, @{nameof(Setting.Value)}",
                    (result, next) => $"{result}, @{next}"
                );

                sql.Append($"VALUES ({parameterNames})");
            }

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters

            AddParameter(command, nameof(Setting.Path), settingPath.ToString("a.b[]", SettingPathFormatter.Instance));
            AddParameter(command, nameof(Setting.Value), value);
            AddParameters(command, where);

            return command;
        }

        private void AddParameter(SqlCommand command, string name, object value = null)
        {
            if (!_tableMetadata.Columns.TryGetValue(name, out ColumnMetadata<SqlDbType> column))
            {
                throw new ColumnConfigurationNotFoundException(name);
            }

            var parameter = command.Parameters.Add(name, column.DbType, column.Length);

            if (value != null)
            {
                parameter.Value = value;
            }
        }

        private void AddParameters(SqlCommand command, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            foreach (var parameter in parameters)
            {
                AddParameter(command, parameter.Key, parameter.Value);
            }
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
}
