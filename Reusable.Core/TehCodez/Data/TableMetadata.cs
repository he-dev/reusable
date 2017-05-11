using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Extensions;

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

        //public static readonly TableMetadata<TDbType> Empty = Create(null, null);

        public TableMetadata<TDbType> AddColumn(string name, TDbType dbType, int length)
        {
            var column = new ColumnMetadata<TDbType>(
                name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name)),
                dbType,
                length);

            return new TableMetadata<TDbType>(
                SchemaName,
                TableName,
                Columns.Add(column.Name, column)
            );
        }

        public static TableMetadata<TDbType> Create(string schemaName, string tableName)
        {
            return new TableMetadata<TDbType>(
                schemaName.NullIfEmpty() ?? throw new ArgumentNullException(nameof(schemaName)),
                tableName.NullIfEmpty() ?? throw new ArgumentNullException(nameof(tableName)),
                ImmutableDictionary<string, ColumnMetadata<TDbType>>.Empty);
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
