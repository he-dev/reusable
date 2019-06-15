using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Utilities.SqlClient;

namespace Reusable.IOnymous.Config
{
    internal static class SqlCommandFactory
    {
        public static SqlCommand CreateSelectCommand
        (
            [NotNull] this SqlConnection connection,
            [NotNull] SqlFourPartName tableName,
            [NotNull] string name,
            [CanBeNull] IImmutableDictionary<SqlServerColumn, SoftString> columnMappings,
            [CanBeNull] IImmutableDictionary<string, object> where,
            (string Name, object Value) fallback)
        {
            where = where ?? ImmutableDictionary<string, object>.Empty;

            var sql = new StringBuilder();

            var table = tableName.Render(connection);

            /*
                select * from #test
                where 
                    _name = 'a' and
                    _ver = '1' and
                    (_env = 'd' or (_env = 'p' and not exists (select 1 from #test where _name = 'a' and _env = 'd' and _ver = '1')))
              
             */

            var nameCondition = $"[{columnMappings.MapOrDefault(SqlServerColumn.Name)}] = @{columnMappings.MapOrDefault(SqlServerColumn.Name)}";
            //var fallback = (Name: "env", Value: "prod");

            sql.Append($"select *").AppendLine();
            sql.Append($"from {table}").AppendLine();
            sql.Append(where.Aggregate($"where {nameCondition}", (current, next) =>
            {
                var column = connection.CreateIdentifier(next.Key);
                var defaultParam = next.Key;
                var fallbackParam = next.Key + "_fallback";
                if (next.Key == fallback.Name)
                {
                    var fallbackCondition = where.Aggregate($"where {nameCondition}", (c, n) => $"{c} and {connection.CreateIdentifier(n.Key)} = @{n.Key}");
                    return $"{current} and ({column} = @{defaultParam} or ({column} = @{fallbackParam} and not exists (select 1 from {table} {fallbackCondition})))";
                }
                else
                {
                    return $"{current} and {connection.CreateIdentifier(next.Key)} = @{next.Key}";
                }
            }));

            var command = connection.CreateCommand();
            command.CommandText = sql.ToString();

            // --- add parameters & values

            command.AddParameters(where.Add(columnMappings.MapOrDefault(SqlServerColumn.Name), name));
            if (fallback.Name.IsNotNullOrEmpty())
            {
                command.Parameters.AddWithValue($"@{fallback.Name}_fallback", fallback.Value);
            }

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

        public static SqlCommand CreateUpdateCommand
        (
            [NotNull] this SqlConnection connection,
            [NotNull] SqlFourPartName tableName,
            [NotNull] string name,
            [CanBeNull] IImmutableDictionary<SqlServerColumn, SoftString> columnMappings,
            [CanBeNull] IImmutableDictionary<string, object> @where,
            [CanBeNull] object value)
        {
            where = where ?? ImmutableDictionary<string, object>.Empty;

            /*
             
            UPDATE [Setting]
	            SET [Value] = 'Hallo update!'
	            WHERE [Name]='baz' AND [Environment] = 'boz'
            IF @@ROWCOUNT = 0 
	            INSERT INTO [Setting]([Name], [Value], [Environment])
	            VALUES ('baz', 'Hallo insert!', 'boz')
            
            */

            var sql = new StringBuilder();

            var table = tableName.Render(connection);

            sql.Append($"UPDATE {table}").AppendLine();
            sql.Append($"SET [{columnMappings.MapOrDefault(SqlServerColumn.Value)}] = @{columnMappings.MapOrDefault(SqlServerColumn.Value)}").AppendLine();

            sql.Append(where.Aggregate(
                $"WHERE [{columnMappings.MapOrDefault(SqlServerColumn.Name)}] = @{columnMappings.MapOrDefault(SqlServerColumn.Name)}",
                (result, next) => $"{result} AND {connection.CreateIdentifier(next.Key)} = @{next.Key} ")
            ).AppendLine();

            sql.Append($"IF @@ROWCOUNT = 0").AppendLine();

            var columns = where.Keys.Select(key => connection.CreateIdentifier(key)).Aggregate(
                $"[{columnMappings.MapOrDefault(SqlServerColumn.Name)}], [{columnMappings.MapOrDefault(SqlServerColumn.Value)}]",
                (result, next) => $"{result}, {next}"
            );

            sql.Append($"INSERT INTO {table}({columns})").AppendLine();

            var parameterNames = where.Keys.Aggregate(
                $"@{columnMappings.MapOrDefault(SqlServerColumn.Name)}, @{columnMappings.MapOrDefault(SqlServerColumn.Value)}",
                (result, next) => $"{result}, @{next}"
            );

            sql.Append($"VALUES ({parameterNames})");

            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql.ToString();

            // --- add parameters

            command.Parameters.AddWithValue($"@{columnMappings.MapOrDefault(SqlServerColumn.Name)}", name);
            command.Parameters.AddWithValue($"@{columnMappings.MapOrDefault(SqlServerColumn.Value)}", value);
            //command.Parameters.Add($"@{sqlServer.ColumnMapping.Value}", SqlDbType.NVarChar, 200).Value = setting.Value;

            command.AddParameters(where);

            return command;
        }

        private static string CreateParameterNames<T>(this IEnumerable<T> values, string name)
        {
            return string.Join(", ", values.Select((x, i) => $"@{name}_{i}"));
        }
    }

    internal static class SqlCommandExtensions
    {
        public static SqlCommand AddParameters(this SqlCommand cmd, IReadOnlyDictionary<string, object> parameters)
        {
            foreach (var parameter in parameters)
            {
                cmd.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);
            }

            return cmd;
        }

        public static SqlCommand AddParameters(this SqlCommand cmd, IEnumerable<SoftString> values, string name)
        {
            foreach (var (value, index) in values.Select((x, i) => (x, i)))
            {
                cmd.Parameters.AddWithValue($"@{name.ToString()}_{index}", value.ToString());
            }

            return cmd;
        }
    }
}