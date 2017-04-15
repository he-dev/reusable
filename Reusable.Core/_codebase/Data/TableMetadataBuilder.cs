using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Reusable.Collections;
using Reusable.Data;

namespace Reusable.Data
{
    public class TableMetadataBuilder<TDbType>
    {
        private string _schemaName;
        private string _tableName;
        private AutoKeyDictionary<string, ColumnMetadata<TDbType>> _columns = new AutoKeyDictionary<string, ColumnMetadata<TDbType>>(x => x.Name, StringComparer.OrdinalIgnoreCase);

        private TableMetadataBuilder() { }

        public static TableMetadataBuilder<TDbType> Create() => new TableMetadataBuilder<TDbType>();

        public TableMetadataBuilder<TDbType> SchemaName(string schemaName)
        {
            _schemaName = schemaName;
            return this;
        }

        public TableMetadataBuilder<TDbType> TableName(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public TableMetadataBuilder<TDbType> Column(string name, TDbType sqlDbType, int length)
        {
            var column = new ColumnMetadata<TDbType>(name, sqlDbType, length);
            _columns.Remove(column);
            _columns.Add(column);
            return this;
        }

        public TableMetadataBuilder<TDbType> Column<T>(Expression<Func<T>> expression, TDbType sqlDbType, int length)
        {
            var memberExpression = ((expression ?? throw new ArgumentNullException(nameof(expression))).Body as MemberExpression) ?? throw new ArgumentException(nameof(expression), $"The expression must be a {nameof(MemberExpression)}.");
            return Column(memberExpression.Member.Name, sqlDbType, length);
        }

        public TableMetadata<TDbType> Build()
        {
            return new TableMetadata<TDbType>(_schemaName, _tableName, _columns);
        }
    }
}
