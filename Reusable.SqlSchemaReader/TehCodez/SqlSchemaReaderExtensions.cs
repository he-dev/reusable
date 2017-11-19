using System.Collections.Generic;
using System.Data.SqlClient;
using Reusable.Extensions;

namespace Reusable.Data.SqlClient
{
    public static class SqlSchemaReaderExtensions
    {
        public static List<SqlTableSchema> GetSqlTableSchemas(this ISqlSchemaReader reader, string connectionString, SqlTableSchema schemaRestriction)
        {
            using (var sqlConnection = new SqlConnection(connectionString).Then(c => c.Open()))
            {
                return reader.GetSqlTableSchemas(sqlConnection, schemaRestriction);
            }
        }

        public static List<SqlColumnSchema> GetSqlColumnSchemas(this ISqlSchemaReader reader, string connectionString, SqlColumnSchema schemaRestriction)
        {
            using (var sqlConnection = new SqlConnection(connectionString).Then(c => c.Open()))
            {
                return reader.GetSqlColumnSchemas(sqlConnection, schemaRestriction);
            }
        }
    }
}