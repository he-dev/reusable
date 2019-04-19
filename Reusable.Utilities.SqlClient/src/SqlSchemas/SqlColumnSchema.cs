using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.Utilities.SqlClient.SqlSchemas
{
    [UsedImplicitly, PublicAPI, DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SqlColumnSchema
    {
        // Based on https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings
        private static readonly IDictionary<SqlDbType, Type> SqlDbTypeMap = new Dictionary<SqlDbType, Type>
        {
            [SqlDbType.BigInt] = typeof(long),
            [SqlDbType.Binary] = typeof(byte[]),
            [SqlDbType.Bit] = typeof(bool),
            [SqlDbType.Char] = typeof(string),
            [SqlDbType.Date] = typeof(DateTime),
            [SqlDbType.DateTime] = typeof(DateTime),
            [SqlDbType.DateTime2] = typeof(DateTime),
            [SqlDbType.DateTimeOffset] = typeof(DateTimeOffset),
            [SqlDbType.Decimal] = typeof(decimal),
            [SqlDbType.Float] = typeof(float),
            [SqlDbType.Image] = typeof(byte[]),
            [SqlDbType.Int] = typeof(int),
            [SqlDbType.Money] = typeof(decimal),
            [SqlDbType.NChar] = typeof(string),
            [SqlDbType.NText] = typeof(string),
            [SqlDbType.NVarChar] = typeof(string),
            [SqlDbType.Real] = typeof(float),
            [SqlDbType.SmallDateTime] = typeof(DateTime),
            [SqlDbType.SmallInt] = typeof(short),
            [SqlDbType.SmallMoney] = typeof(decimal),
            [SqlDbType.Text] = typeof(string),
            [SqlDbType.Time] = typeof(TimeSpan),
            [SqlDbType.Timestamp] = typeof(byte[]),
            [SqlDbType.TinyInt] = typeof(byte),
            [SqlDbType.UniqueIdentifier] = typeof(Guid),
            [SqlDbType.VarBinary] = typeof(byte[]),
            [SqlDbType.VarChar] = typeof(string),
            [SqlDbType.Xml] = typeof(System.Data.SqlTypes.SqlXml),
            // what do we do with these?
            // rowversion
            // varbinary(maX)
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
        [CanBeNull]
        [NotMapped]
        public Type FrameworkType
        {
            get
            {
                return
                    Enum.TryParse<SqlDbType>(DataType, true, out var sqlDbType)
                        ? SqlDbTypeMap[sqlDbType]
                        : null;
            }
        }

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