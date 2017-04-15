using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Data
{
    public class TableMetadata<TDbType>
    {
        public TableMetadata(string schemaName, string tableName, IImmutableDictionary<string, ColumnMetadata<TDbType>> columns)
        {
            SchemaName = schemaName;
            TableName = tableName;
            Columns = columns;
        }

        public string SchemaName { get; }

        public string TableName { get; }

        public IImmutableDictionary<string, ColumnMetadata<TDbType>> Columns { get; }

        public static readonly TableMetadata<TDbType> Empty = Create(null, null);

        public TableMetadata<TDbType> AddColumn(string name, TDbType dbType, int length)
        {
            return new TableMetadata<TDbType>(
                SchemaName,
                TableName,
                Columns.Add(name, new ColumnMetadata<TDbType>(name, dbType, length))
            );
        }

        public static TableMetadata<TDbType> Create(string schemaName, string tableName)
        {
            return new TableMetadata<TDbType>(schemaName, tableName, ImmutableDictionary<string, ColumnMetadata<TDbType>>.Empty);
        }
    }

    public class ColumnMetadata<TDbType>
    {
        public ColumnMetadata(string name, TDbType dbType, int length)
        {
            Name = name;
            DbType = dbType;
            Length = length;
        }
        public string Name { get; }
        public TDbType DbType { get; }
        public int Length { get; }
    }
}
