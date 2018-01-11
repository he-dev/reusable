using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Reflection;

namespace Reusable.Data.SqlClient
{
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
        public static IList<(string Name, Type Type)> GetColumnFrameworkTypes(this SqlConnection connection, [NotNull] string schema, [NotNull] string table)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (table == null) throw new ArgumentNullException(nameof(table));

            return connection.GetColumnSchemas(new SqlColumnSchema
            {
                TableSchema = schema,
                TableName = table
            })
            .Select((column, ordinal) => (column.ColumnName, column.FrameworkType))
            .ToList();
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