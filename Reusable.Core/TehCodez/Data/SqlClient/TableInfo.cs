using System.Data;

namespace Reusable.Data.SqlClient
{
    public class TableInfo : SchemaInfo
    {
        public TableInfo(DataRow row) : base(row) { }

        [SchemaColumnName("table_catalog")]
        public string TableCatalog { get; set; }

        [SchemaColumnName("table_schema")]
        public string TableSchema { get; set; }

        [SchemaColumnName("table_name")]
        public string TableName { get; set; }

        [SchemaColumnName("table_type")]
        public string TableType { get; set; }
    }
}