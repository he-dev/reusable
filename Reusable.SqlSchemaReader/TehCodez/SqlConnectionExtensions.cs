using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Data.SqlClient
{
    public static class SqlConnectionExtensions
    {
        private static readonly ISqlSchemaReader SqlSchemaReader = new SqlSchemaReader();

        [NotNull, ItemNotNull]
        public static List<SqlTableSchema> GetSqlTableSchemas(this SqlConnection sqlConnection, SqlTableSchema schemaRestriction)
        {
            return SqlSchemaReader.GetSqlTableSchemas(sqlConnection, schemaRestriction);                       
        }

        [NotNull, ItemNotNull]
        public static List<SqlColumnSchema> GetSqlColumnSchemas(this SqlConnection sqlConnection, SqlColumnSchema schemaRestriction)
        {
            return SqlSchemaReader.GetSqlColumnSchemas(sqlConnection, schemaRestriction);
        }
    }
}