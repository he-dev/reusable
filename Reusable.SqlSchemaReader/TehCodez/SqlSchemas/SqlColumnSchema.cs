using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.Data.SqlClient
{
    [UsedImplicitly, PublicAPI, DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class  SqlColumnSchema
    {
        private static readonly IReadOnlyDictionary<SqlDbType, Type> SqlDbTypeMap = new Dictionary<SqlDbType, Type>
        {
            [SqlDbType.NVarChar] = typeof(string),
            [SqlDbType.Bit] = typeof(bool),
        };

        private string DebuggerDisplay => $"[{TableCatalog}].[{TableSchema}].[{TableName}].[{ColumnName}] ({DataType})";

        public string TableCatalog { get; set; }

        public string TableSchema { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public int OrdinalPosition { get; set; }

        public string ColumnDefault { get; set; }

        public string IsNullable { get; set; }

        public string DataType { get; set; }

        /// <summary>
        /// Gets the corresponding .NET Framework type.
        /// </summary>
        [NotMapped]
        public Type FrameworkType => SqlDbTypeMap[(SqlDbType)Enum.Parse(typeof(SqlDbType), DataType, ignoreCase: true)];

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