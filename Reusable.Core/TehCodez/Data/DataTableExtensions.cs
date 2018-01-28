using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Data
{
    public static class DataTableExtensions
    {
        public static DataTable AddRow([NotNull] this DataTable dataTable, [NotNull] params object[] values)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));

            var newRow = dataTable.NewRow();
            newRow.ItemArray = values;
            dataTable.Rows.Add(newRow);
            return dataTable;
        }

        public static DataTable AddColumn([NotNull] this DataTable dataTable, [NotNull] string columnName, [CanBeNull] Action<DataColumn> setupColumn = null)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));
            if (columnName == null) throw new ArgumentNullException(nameof(columnName));

            var column = dataTable.Columns.Add(columnName);
            setupColumn?.Invoke(column);
            return dataTable;
        }

        public static DataTable AddColumn([NotNull] this DataTable dataTable, [NotNull] string columnName, Type dataType)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));
            if (columnName == null) throw new ArgumentNullException(nameof(columnName));

            return dataTable.AddColumn(columnName, column => column.DataType = dataType);
        }
    }
}
