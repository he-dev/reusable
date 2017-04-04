using System.Data;

namespace Reusable.Data.SqlClient
{
    public class ColumnInfo : SchemaInfo
    {
        public ColumnInfo(DataRow row) : base(row) { }

        [SchemaColumnName("column_name")]
        public string ColumnName { get; set; }

        [SchemaColumnName("ordinal_position")]
        public int OrdinalPosition { get; set; }

        [SchemaColumnName("column_default")]
        public string ColumnDefault { get; set; }

        [SchemaColumnName("is_nullable")]
        public string IsNullable { get; set; }

        [SchemaColumnName("data_type")]
        public string DataType { get; set; }

        [SchemaColumnName("character_maximum_length")]
        public int CharacterMaximumLength { get; set; }

        [SchemaColumnName("numeric_precision")]
        public byte NumericPrecision { get; set; }

        [SchemaColumnName("numeric_precision_radix")]
        public short? NumericPrecisionRadix { get; set; }

        [SchemaColumnName("numeric_scale")]
        public int? NumericScale { get; set; }

        [SchemaColumnName("datetime_precision")]
        public short? DatetimePrecision { get; set; }

        [SchemaColumnName("character_set_name")]
        public string CharacterSetName { get; set; }
    }
}