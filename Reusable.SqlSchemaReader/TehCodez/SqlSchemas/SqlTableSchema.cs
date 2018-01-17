using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.Utilities.SqlClient.SqlSchemas
{
    [UsedImplicitly, PublicAPI, DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SqlTableSchema
    {
        private string DebuggerDisplay => $"[{TableCatalog}].[{TableSchema}].[{TableName}] ({TableType})";

        [ColumnOrdinal(0)]
        public string TableCatalog { get; set; }

        [ColumnOrdinal(1)]
        public string TableSchema { get; set; }

        [ColumnOrdinal(2)]
        public string TableName { get; set; }

        [ColumnOrdinal(3)]
        public string TableType { get; set; }

        public static implicit operator string[] (SqlTableSchema schema) => new[]
        {
            schema.TableCatalog,
            schema.TableSchema,
            schema.TableName,
            schema.TableType
        };
    }
}