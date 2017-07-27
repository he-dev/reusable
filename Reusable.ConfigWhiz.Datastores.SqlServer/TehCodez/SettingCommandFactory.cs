using System.Collections.Generic;
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
    internal static class SettingCommandFactory
    {
        public static SqlCommand CreateSelectCommand(this SqlConnection connection, SqlServer datastore, IEnumerable<CaseInsensitiveString> names)
        {
            var sql = new StringBuilder();

            var dbProviderFactory = DbProviderFactories.GetFactory(connection);
            using (var commandBuilder = dbProviderFactory.CreateCommandBuilder())
            {
                string Sanitize(string identifier) => commandBuilder.QuoteIdentifier(identifier);

                var table = $"{Sanitize(datastore.Schema)}.{Sanitize(datastore.Table)}";

                sql.Append($"SELECT *").AppendLine();
                sql.Append($"FROM {table}").AppendLine();
                sql.Append(datastore.Where.Aggregate(
                    $"WHERE [{nameof(ISetting.Name)}] IN ({names.CreateParameterNames(nameof(ISetting.Name))})",
                    (current, next) => $"{current} AND {Sanitize(next.Key)} = @{next.Key}")
                );
            }

            var command = connection.CreateCommand();
            command.CommandText = sql.ToString();

            // --- add parameters & values

            command
                .AddParameters(names, nameof(ISetting.Name))
                .AddParameters(datastore.Where);            

            return command;
        }

        ////public static SqlCommand CreateDeleteCommand(SqlConnection connection, IIdentifier id, IImmutableDictionary<string, object> where)
        ////{
        ////    /*
             
        ////    DELETE FROM [dbo].[Setting] WHERE [Name] LIKE 'baz%' AND [Environment] = 'boz'

        ////    */

        ////    var sql = new StringBuilder();

        ////    var dbProviderFactory = DbProviderFactories.GetFactory(connection);
        ////    using (var commandBuilder = dbProviderFactory.CreateCommandBuilder())
        ////    {
        ////        string Sanitize(string identifier) => commandBuilder.QuoteIdentifier(identifier);

        ////        var table = $"{Sanitize(_tableMetadata.SchemaName)}.{Sanitize(_tableMetadata.TableName)}";

        ////        sql.Append($"DELETE FROM {table}").AppendLine();
        ////        sql.Append(where.Keys.Aggregate(
        ////            $"WHERE ([{EntityProperty.Name}] = @{EntityProperty.Name} OR [{EntityProperty.Name}] LIKE @{EntityProperty.Name} + N'[[]%]')",
        ////            (result, next) => $"{result} AND {Sanitize(next)} = @{next} ")
        ////        );
        ////    }

        ////    var command = connection.CreateCommand();
        ////    command.CommandType = CommandType.Text;
        ////    command.CommandText = sql.ToString();

        ////    // --- add parameters & values

        ////    (command, _tableMetadata).AddParameter(
        ////        ImmutableDictionary<string, object>.Empty
        ////            .Add(EntityProperty.Name, id.ToString())
        ////            .AddRange(where)
        ////    );

        ////    return command;
        ////}

        public static SqlCommand CreateUpdateCommand(this SqlConnection connection, SqlServer datastore, ISetting setting)
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

                var table = $"{Sanitize(datastore.Schema)}.{Sanitize(datastore.Table)}";

                sql.Append($"UPDATE {table}").AppendLine();
                sql.Append($"SET [{nameof(ISetting.Value)}] = @{nameof(ISetting.Value)}").AppendLine();

                sql.Append(datastore.Where.Aggregate(
                    $"WHERE [{nameof(ISetting.Name)}] = @{nameof(ISetting.Name)}",
                    (result, next) => $"{result} AND {Sanitize(next.Key)} = @{next.Key} ")
                ).AppendLine();

                sql.Append($"IF @@ROWCOUNT = 0").AppendLine();

                var columns = datastore.Where.Keys.Select(Sanitize).Aggregate(
                    $"[{nameof(ISetting.Name)}], [{nameof(ISetting.Value)}]", 
                    (result, next) => $"{result}, {next}"
                );

                sql.Append($"INSERT INTO {table}({columns})").AppendLine();

                var parameterNames = datastore.Where.Keys.Aggregate(
                    $"@{nameof(ISetting.Name)}, @{nameof(ISetting.Value)}",
                    (result, next) => $"{result}, @{next}"
                );

                sql.Append($"VALUES ({parameterNames})");
            }

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters

            command.Parameters.AddWithValue(nameof(ISetting.Name), setting.Name.ToString());
            command.Parameters.AddWithValue(nameof(ISetting.Value), setting.Value);

            command.AddParameters(datastore.Where);            

            return command;
        }

        private static string CreateParameterNames<T>(this IEnumerable<T> values, string name)
        {
            return string.Join(", ", values.Select((x, i) => $"@{name}_{i}"));
        }
    }

    internal static class SqlCommandExtensions
    {
        public static SqlCommand AddParameters(this SqlCommand cmd, IImmutableDictionary<string, object> parameters)
        {
            foreach (var parameter in parameters)
            {
                cmd.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);
            }
            return cmd;
        }

        public static SqlCommand AddParameters(this SqlCommand cmd, IEnumerable<CaseInsensitiveString> values, string name)
        {
            foreach (var t in values.Select((x, i) => (Value: x, Index: i)))
            {
                cmd.Parameters.AddWithValue($"@{name.ToString()}_{t.Index}", t.Value.ToString());
            }
            return cmd;
        }
    }
}