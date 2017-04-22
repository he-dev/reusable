using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.ConfigWhiz.Data;
using Reusable.Data;

namespace Reusable.ConfigWhiz.Datastores
{
    internal class SettingCommandFactory
    {
        private readonly TableMetadata<DbType> _tableMetadata;

        public SettingCommandFactory(TableMetadata<DbType> tableMetadata)
        {
            _tableMetadata = tableMetadata;
        }

        public SQLiteCommand CreateSelectCommand(SQLiteConnection connection, SettingPath settingPath, IImmutableDictionary<string, object> where)
        {
            // --- build sql

            // SELECT * FROM {table} WHERE [Name] = '{name}' AND 'Foo' = 'bar'

            var sql = new StringBuilder();

            var dbProviderFactory = DbProviderFactories.GetFactory(connection);
            using (var commandBuilder = dbProviderFactory.CreateCommandBuilder())
            {
                string Sanitize(string identifier) => commandBuilder.QuoteIdentifier(identifier);

                var table = $"{Sanitize(_tableMetadata.TableName)}";

                sql.Append($"SELECT * FROM {table}").AppendLine();
                sql.Append(where.Keys.Aggregate(
                    $"WHERE ([{SettingProperty.Name}] = @{SettingProperty.Name} OR [{SettingProperty.Name}] LIKE @{SettingProperty.Name} || '[%]')",
                    (result, key) => $"{result} AND {Sanitize(key)} = @{key}")
                );
            }

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters

            (command, _tableMetadata).AddParameter(
                ImmutableDictionary<string, object>.Empty
                    .Add(SettingProperty.Name, settingPath.ToFullWeakString())
                    .AddRange(where));

            return command;
        }

        public SQLiteCommand CreateDeleteCommand(SQLiteConnection connection, SettingPath settingPath, IImmutableDictionary<string, object> where)
        {
            /*
             
            DELETE FROM [dbo].[Setting] WHERE [Name] LIKE 'baz%' AND [Environment] = 'boz'

            */

            var sql = new StringBuilder();

            var dbProviderFactory = DbProviderFactories.GetFactory(connection);
            using (var commandBuilder = dbProviderFactory.CreateCommandBuilder())
            {
                string Sanitize(string identifier) => commandBuilder.QuoteIdentifier(identifier);

                var table = $"{Sanitize(_tableMetadata.TableName)}";

                sql.Append($"DELETE FROM {table}").AppendLine();
                sql.Append(where.Keys.Aggregate(
                    $"WHERE ([{SettingProperty.Name}] = @{SettingProperty.Name} OR [{SettingProperty.Name}] LIKE @{SettingProperty.Name} || '[%]')",
                    (result, key) => $"{result} AND {Sanitize(key)} = @{key} ")
                );
            }

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters & values
     
            (command, _tableMetadata).AddParameter(
                ImmutableDictionary<string, object>.Empty
                    .Add(SettingProperty.Name, settingPath.ToFullWeakString())
                    .AddRange(where));

            return command;
        }

        public SQLiteCommand CreateInsertCommand(SQLiteConnection connection, SettingPath settingPath, object value, IImmutableDictionary<string, object> where)
        {
            /*
                INSERT OR REPLACE INTO Setting([Name], [Value])
                VALUES('{setting.Name.FullNameEx}', '{setting.Value}')
            */

            // --- build sql

            var sql = new StringBuilder();

            var dbProviderFactory = DbProviderFactories.GetFactory(connection);
            using (var commandBuilder = dbProviderFactory.CreateCommandBuilder())
            {
                string Sanitize(string identifier) => commandBuilder.QuoteIdentifier(identifier);

                var table = $"{Sanitize(_tableMetadata.TableName)}";

                var columns = where.Keys.Select(Sanitize).Aggregate(
                    $"[{SettingProperty.Name}], [{SettingProperty.Value}]",
                    (result, next) => $"{result}, {next}"
                );

                sql.Append($"INSERT OR REPLACE INTO {table}({columns})").AppendLine();

                var parameterNames = where.Keys.Aggregate(
                    $"@{SettingProperty.Name}, @{SettingProperty.Value}",
                    (result, next) => $"{result}, @{next}"
                );

                sql.Append($"VALUES ({parameterNames})").AppendLine();
            }

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters

            (command, _tableMetadata).AddParameter(
                ImmutableDictionary<string, object>.Empty
                    .Add(SettingProperty.Name, settingPath.ToFullStrongString())
                    .Add(SettingProperty.Value, value)
                    .AddRange(where));

            return command;
        }

    }

    public class ColumnConfigurationNotFoundException : Exception
    {
        internal ColumnConfigurationNotFoundException(string column)
        {
            Column = column;
        }

        public string Column { get; set; }

        public override string Message => $"\"{Column}\" column configuration not found. Ensure that it is set via the \"{nameof(SQLite)}\" builder.";
    }

    internal static class SqlCommandExtensions
    {
        public static (SQLiteCommand cmd, TableMetadata<DbType> tableMetadata) AddParameter(this (SQLiteCommand cmd, TableMetadata<DbType> tableMetadata) @this, IImmutableDictionary<string, object> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (@this.tableMetadata.Columns.TryGetValue(parameter.Key, out ColumnMetadata<DbType> column))
                {
                    var sqlParameter = @this.cmd.Parameters.Add($"@{parameter.Key}", column.DbType, column.Length);
                    if (parameter.Value != null) sqlParameter.Value = parameter.Value;
                }
                else
                {
                    throw new ColumnConfigurationNotFoundException(parameter.Key);
                }
            }
            return @this;
        }
    }
}
