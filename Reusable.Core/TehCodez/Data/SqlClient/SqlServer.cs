using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Data.SqlClient
{
    public class SqlServer
    {
        public List<TableInfo> GetTables(string connectionString, string database, string tableSchema)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (tableSchema == null) throw new ArgumentNullException(nameof(tableSchema));

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.GetTables(database, tableSchema);
            }
        }

        public List<ColumnInfo> GetColumns(string connectionString, string tableSchema, string tableName)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (tableSchema == null) throw new ArgumentNullException(nameof(tableSchema));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.GetColumns(tableSchema, tableName);
            }
        }
    }
}
