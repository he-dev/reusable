using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Data.SqlClient
{
    public static class SqlSchemaReaderExtensions
    {
        public static IList<SqlTableSchema> GetSqlTableSchemas(this ISqlSchema reader, string connectionString, SqlTableSchema schemaRestriction)
        {
            using (var sqlConnection = new SqlConnection(connectionString).Then(c => c.Open()))
            {
                return reader.GetTableSchemas(sqlConnection, schemaRestriction);
            }
        }

        public static IList<SqlColumnSchema> GetSqlColumnSchemas(this ISqlSchema reader, string connectionString, SqlColumnSchema schemaRestriction)
        {
            using (var sqlConnection = new SqlConnection(connectionString).Then(c => c.Open()))
            {
                return reader.GetColumnSchemas(sqlConnection, schemaRestriction);
            }
        }

        [NotNull]
        public static IDictionary<string, Type> GetColumnFrameworkTypes([NotNull] this ISqlSchema reader, [NotNull] string connectionString, [NotNull] string schema, [NotNull] string table)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (table == null) throw new ArgumentNullException(nameof(table));

            return reader.GetSqlColumnSchemas(connectionString, new SqlColumnSchema
            {
                TableSchema = schema,
                TableName = table
            })
            .ToDictionary(x => x.ColumnName, x => x.FrameworkType, StringComparer.OrdinalIgnoreCase);
        }
    }
}