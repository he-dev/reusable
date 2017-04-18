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
                var quote = new Func<string, string>(identifier => commandBuilder.QuoteIdentifier(identifier));

                var table = $"{quote(_tableMetadata.TableName)}";

                sql.Append($"SELECT * FROM {table}").AppendLine();
                sql.Append(where.Keys.Aggregate(
                    $"WHERE ([{SettingProperty.Name}] = @{SettingProperty.Name} OR [{SettingProperty.Name}] LIKE @{SettingProperty.Name} || '[%]')",
                    (result, key) => $"{result} AND {quote(key)} = @{key}")
                );
            }

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters

            AddParameter(command, SettingProperty.Name, settingPath.ToFullWeakString());
            AddParameters(command, where);

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
                var quote = new Func<string, string>(identifier => commandBuilder.QuoteIdentifier(identifier));

                var table = $"{quote(_tableMetadata.TableName)}";

                sql.Append($"DELETE FROM {table}").AppendLine();
                sql.Append(where.Keys.Aggregate(
                    $"WHERE ([{SettingProperty.Name}] = @{SettingProperty.Name} OR [{SettingProperty.Name}] LIKE @{SettingProperty.Name} || '[%]')",
                    (result, key) => $"{result} AND {quote(key)} = @{key} ")
                );
            }

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters & values

            AddParameter(command, SettingProperty.Name, settingPath.ToFullWeakString());
            AddParameters(command, where);

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
                var quote = new Func<string, string>(identifier => commandBuilder.QuoteIdentifier(identifier));

                var table = $"{quote(_tableMetadata.TableName)}";

                var columns = where.Keys.Select(columnName => quote(columnName)).Aggregate(
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

            AddParameter(command, SettingProperty.Name, settingPath.ToFullStrongString());
            AddParameter(command, SettingProperty.Value, value);
            AddParameters(command, where);

            return command;
        }

        private void AddParameter(SQLiteCommand command, string name, object value = null)
        {
            if (!_tableMetadata.Columns.TryGetValue(name, out ColumnMetadata<DbType> column))
            {
                throw new ColumnConfigurationNotFoundException(name);
            }

            var parameter = command.Parameters.Add(name, column.DbType, column.Length);

            if (value != null)
            {
                parameter.Value = value;
            }
        }

        private void AddParameters(SQLiteCommand command, IEnumerable<KeyValuePair<string, object>> parameters)
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

        public override string Message => $"\"{Column}\" column configuration not found. Ensure that it is set via the \"{nameof(SQLite)}\" builder.";
    }
}
