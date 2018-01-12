using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Utilities.SqlClient
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class NamespaceProvider { }

    public static class SqlConnectionExtensions
    {
        private static readonly string GetIdentityColumnsQuery = ResourceReader<NamespaceProvider>.FindString(name => name.EndsWith($"{nameof(GetIdentityColumns)}.sql"));

        public static IList<SqlTableSchema> GetTableSchemas(this SqlConnection sqlConnection, SqlTableSchema schemaRestriction)
        {
            if (sqlConnection == null) throw new ArgumentNullException(nameof(sqlConnection));
            if (schemaRestriction == null) throw new ArgumentNullException(nameof(schemaRestriction));

            using (var schema = sqlConnection.GetSchema(SqlSchemaCollection.Tables, schemaRestriction))
            {
                return
                    schema
                        .AsEnumerable()
                        .Select(SqlSchemaFactory.Create<SqlTableSchema>)
                        .ToList();
            }
        }

        /// <summary>
        /// Gets column schemas ordered by their ordinal-position.
        /// </summary>
        public static IList<SqlColumnSchema> GetColumnSchemas(this SqlConnection sqlConnection, SqlColumnSchema schemaRestriction)
        {
            if (sqlConnection == null) throw new ArgumentNullException(nameof(sqlConnection));
            if (schemaRestriction == null) throw new ArgumentNullException(nameof(schemaRestriction));

            using (var schema = sqlConnection.GetSchema(SqlSchemaCollection.Columns, schemaRestriction))
            {
                return
                    schema
                        .AsEnumerable()
                        .Select(SqlSchemaFactory.Create<SqlColumnSchema>)
                        .OrderBy(x => x.OrdinalPosition)
                        .ToList();
            }
        }

        public static IList<SqlIdentityColumnSchema> GetIdentityColumns(this SqlConnection connection, string schema, string table)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = GetIdentityColumnsQuery;
                cmd.Parameters.AddWithValue("@schema", schema);
                cmd.Parameters.AddWithValue("@table", table);

                using (var reader = cmd.ExecuteReader())
                {
                    var identityColumns = new DataTable("IdentityColumns");
                    identityColumns.Load(reader);
                    return
                        identityColumns
                            .AsEnumerable()
                            .Select(SqlSchemaFactory.Create<SqlIdentityColumnSchema>)
                            .ToList();
                }
            }
        }

        [NotNull]
        public static IList<(SoftString Name, Type Type)> GetColumnFrameworkTypes(this SqlConnection connection, [NotNull] string schema, [NotNull] string table)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (table == null) throw new ArgumentNullException(nameof(table));

            return connection.GetColumnSchemas(new SqlColumnSchema
            {
                TableSchema = schema,
                TableName = table
            })
            .Select((column, ordinal) => (column.ColumnName.ToSoftString(), column.FrameworkType))
            .ToList();
        }

        [NotNull, ContractAnnotation("connection: null => halt")]
        public static string CreateIdentifier([NotNull] this SqlConnection connection, params string[] names)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            using (var commandBuilder = DbProviderFactories.GetFactory(connection).CreateCommandBuilder())
            {
                // ReSharper disable once PossibleNullReferenceException - commandBuilder is never null for SqlConnection.
                return names.Select(commandBuilder.QuoteIdentifier).Join(".");
            }
        }

        public static async Task<IDisposable> ToggleIdentityInsertAsync(this SqlConnection connection, string schema, string table, IEnumerable<SoftString> columns)
        {
            var identityColumns = connection.GetIdentityColumns(schema, table).Select(x => x.Name.ToSoftString()).ToList();
            var containsIdentityColumns = columns.Any(column => identityColumns.Contains(column));

            if (containsIdentityColumns)
            {
                var identifier = connection.CreateIdentifier(schema, table);
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"set identity_insert {identifier} on";
                    await command.ExecuteNonQueryAsync();
                }

                return Disposable.Create(async () =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"set identity_insert {identifier} off";
                        await command.ExecuteNonQueryAsync();
                    }
                });
            }

            return Disposable.Create(() => { });
        }

        public static Task<T> ExecuteQueryAsync<T>(this SqlConnection connection, string query, Func<SqlCommand, Task<T>> body)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                return body(command);
            }
        }

        public static T ExecuteQuery<T>(this SqlConnection connection, string query, Func<SqlCommand, T> body)
        {
            return 
                connection
                    .ExecuteQueryAsync(query, command => Task.FromResult(body(command)))
                    .GetAwaiter()
                    .GetResult();
        }

        //public static async Task<ISet<SoftString>> GetPrimaryKeysAsync(this SqlConnection connection, string schema, string table)
        //{
        //    using (var cmd = connection.CreateCommand())
        //    {
        //        cmd.CommandText = Queries.GetPrimaryKeys;
        //        cmd.Parameters.AddWithValue("@schema", schema);
        //        cmd.Parameters.AddWithValue("@table", table);

        //        using (var reader = await cmd.ExecuteReaderAsync())
        //        {
        //            var keys = new HashSet<SoftString>();

        //            while (reader.Read())
        //            {
        //                keys.Add(await reader.GetFieldValueAsync<string>(0));
        //            }

        //            return keys;
        //        }
        //    }
        //}

        //private static class Queries
        //{
        //    public static readonly string GetPrimaryKeys = ResourceReader<SqlSchema>.FindString(name => name.EndsWith($"{nameof(GetPrimaryKeysAsync)}.sql"));
        //}
    }
}