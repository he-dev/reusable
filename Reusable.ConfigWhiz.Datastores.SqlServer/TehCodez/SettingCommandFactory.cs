using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Reusable.Data;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Datastores
{
    internal class SettingCommandFactory
    {
        private readonly TableMetadata<SqlDbType> _tableMetadata;

        public SettingCommandFactory(TableMetadata<SqlDbType> tableMetadata)
        {
            _tableMetadata = tableMetadata;
        }

        public SqlCommand CreateSelectCommand(SqlConnection connection, IIdentifier id, IImmutableDictionary<string, object> where)
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
                    $"WHERE ([{EntityProperty.Name}] = @{EntityProperty.Name} OR [{EntityProperty.Name}] LIKE @{EntityProperty.Name} + N'[[]%]')",
                    (result, next) => $"{result} AND {Sanitize(next.Key)} = @{next.Key}")
                );
            }

            var command = connection.CreateCommand();
            command.CommandText = sql.ToString();

            // --- add parameters & values

            (command, _tableMetadata).AddParameter(
                ImmutableDictionary<string, object>.Empty
                    .Add(EntityProperty.Name, id.ToString())
                    .AddRange(where)
            );

            return command;
        }

        public SqlCommand CreateDeleteCommand(SqlConnection connection, IIdentifier id, IImmutableDictionary<string, object> where)
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
                    $"WHERE ([{EntityProperty.Name}] = @{EntityProperty.Name} OR [{EntityProperty.Name}] LIKE @{EntityProperty.Name} + N'[[]%]')",
                    (result, next) => $"{result} AND {Sanitize(next)} = @{next} ")
                );
            }

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters & values

            (command, _tableMetadata).AddParameter(
                ImmutableDictionary<string, object>.Empty
                    .Add(EntityProperty.Name, id.ToString())
                    .AddRange(where)
            );

            return command;
        }

        public SqlCommand CreateInsertCommand(SqlConnection connection, IIdentifier id, object value, IImmutableDictionary<string, object> where)
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
                sql.Append($"SET [{EntityProperty.Value}] = @{EntityProperty.Value}").AppendLine();

                sql.Append(where.Keys.Aggregate(
                    $"WHERE ([{EntityProperty.Name}] = @{EntityProperty.Name} OR [{EntityProperty.Name}] LIKE @{EntityProperty.Name} + N'[[]%]')",
                    (result, next) => $"{result} AND {Sanitize(next)} = @{next} ")
                ).AppendLine();

                sql.Append($"IF @@ROWCOUNT = 0").AppendLine();

                var columns = where.Keys.Select(Sanitize).Aggregate(
                    $"[{EntityProperty.Name}], [{EntityProperty.Value}]",
                    (result, next) => $"{result}, {next}"
                );

                sql.Append($"INSERT INTO {table}({columns})").AppendLine();

                var parameterNames = where.Keys.Aggregate(
                    $"@{EntityProperty.Name}, @{EntityProperty.Value}",
                    (result, next) => $"{result}, @{next}"
                );

                sql.Append($"VALUES ({parameterNames})");
            }

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters

            (command, _tableMetadata).AddParameter(
                ImmutableDictionary<string, object>.Empty
                    .Add(EntityProperty.Name, id.ToString())
                    .Add(EntityProperty.Value, value)
                    .AddRange(where));

            return command;
        }       
    }

    internal static class SqlCommandExtensions
    {
        public static (SqlCommand cmd, TableMetadata<SqlDbType> tableMetadata) AddParameter(this (SqlCommand cmd, TableMetadata<SqlDbType> tableMetadata) @this, IImmutableDictionary<string, object> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (@this.tableMetadata.Columns.TryGetValue(parameter.Key, out ColumnMetadata<SqlDbType> column))
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