using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Data.SqlClient
{
    // Based on
    // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections
    
    public interface ISqlSchemaReader
    {
        [NotNull, ItemNotNull]
        List<SqlTableSchema> GetSqlTableSchemas([NotNull] SqlConnection sqlConnection, [NotNull] SqlTableSchema schemaRestriction);
        
        [NotNull, ItemNotNull]
        List<SqlColumnSchema> GetSqlColumnSchemas([NotNull] SqlConnection sqlConnection, [NotNull] SqlColumnSchema schemaRestriction);
    }

    public class SqlSchemaReader : ISqlSchemaReader
    {
        public const string DefaultSchema = "dbo";

        public List<SqlTableSchema> GetSqlTableSchemas(SqlConnection sqlConnection, SqlTableSchema schemaRestriction)
        {
            if (sqlConnection == null) throw new ArgumentNullException(nameof(sqlConnection));
            if (schemaRestriction == null) throw new ArgumentNullException(nameof(schemaRestriction));

            using (var schema = sqlConnection.GetSchema(SqlSchemaCollection.Tables, schemaRestriction))
            {
                return schema.AsEnumerable().Select(SqlSchemaFactory.Create<SqlTableSchema>).ToList();
            }
        }

        public List<SqlColumnSchema> GetSqlColumnSchemas(SqlConnection sqlConnection, SqlColumnSchema schemaRestriction)
        {
            if (sqlConnection == null) throw new ArgumentNullException(nameof(sqlConnection));
            if (schemaRestriction == null) throw new ArgumentNullException(nameof(schemaRestriction));

            using (var schema = sqlConnection.GetSchema(SqlSchemaCollection.Columns, schemaRestriction))
            {
                return schema.AsEnumerable().Select(SqlSchemaFactory.Create<SqlColumnSchema>).ToList();
            }
        }
    }
}