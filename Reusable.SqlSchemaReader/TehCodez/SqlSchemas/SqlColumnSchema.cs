using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.Data.SqlClient
{
    [UsedImplicitly, PublicAPI, DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SqlColumnSchema
    {
        private string DebuggerDisplay => $"[{TableCatalog}].[{TableSchema}].[{TableName}].[{ColumnName}] ({DataType})";

        public string TableCatalog { get; set; }

        public string TableSchema { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public int OrdinalPosition { get; set; }

        public string ColumnDefault { get; set; }

        public string IsNullable { get; set; }

        public string DataType { get; set; }

        public int CharacterMaximumLength { get; set; }

        public byte NumericPrecision { get; set; }

        public short? NumericPrecisionRadix { get; set; }

        public int? NumericScale { get; set; }

        public short? DatetimePrecision { get; set; }

        public string CharacterSetName { get; set; }

        public static implicit operator string[] (SqlColumnSchema schema) => new[]
        {
            schema.TableCatalog,
            schema.TableSchema,
            schema.TableName,
            schema.ColumnName
        };
    }
}