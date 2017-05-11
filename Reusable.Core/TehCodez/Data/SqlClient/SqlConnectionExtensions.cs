using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Reusable.Data.SqlClient
{
    public static class SqlConnectionExtensions
    {
        public static List<TableInfo> GetTables(this SqlConnection conn, string database, string tableSchema)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (tableSchema == null) throw new ArgumentNullException(nameof(tableSchema));

            using (var columns = conn.GetSchema(SchemaCollectionNames.Tables, new SchemaRestriction
            {
                Catalog = database,
                Owner = tableSchema
            }))
            {
                return columns.AsEnumerable().Select(x => new TableInfo(x)).ToList();
            }
        }

        public static List<ColumnInfo> GetColumns(this SqlConnection conn, string tableSchema, string tableName)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (tableSchema == null) throw new ArgumentNullException(nameof(tableSchema));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            using (var columns = conn.GetSchema(SchemaCollectionNames.Columns, new SchemaRestriction
            {
                Owner = tableSchema,
                Table = tableName
            }))
            {
                return columns.AsEnumerable().Select(x => new ColumnInfo(x)).ToList();
            }
        }
    }
}