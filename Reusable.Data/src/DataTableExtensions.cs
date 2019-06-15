using System;
using System.Data;
using JetBrains.Annotations;

namespace Reusable.Data
{
    [PublicAPI]
    public static class DataTableExtensions
    {
        #region AddColumn overloads 

        [NotNull, ContractAnnotation("dataTable: null => halt")]
        public static DataTable AddColumn([NotNull] this DataTable dataTable, [NotNull] string columnName, [CanBeNull] Action<DataColumn> setupColumn = null)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));
            if (columnName == null) throw new ArgumentNullException(nameof(columnName));

            var column = dataTable.Columns.Add(columnName);
            setupColumn?.Invoke(column);
            return dataTable;
        }

        [NotNull, ContractAnnotation("dataTable: null => halt")]
        public static DataTable AddColumn([NotNull] this DataTable dataTable, [NotNull] string columnName, Type dataType)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));
            if (columnName == null) throw new ArgumentNullException(nameof(columnName));

            return dataTable.AddColumn(columnName, column => column.DataType = dataType);
        }

        #endregion

        [NotNull, ContractAnnotation("dataTable: null => halt")]
        public static DataTable AddRow([NotNull] this DataTable dataTable, [NotNull] params object[] values)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));

            var newRow = dataTable.NewRow();
            newRow.ItemArray = values;
            dataTable.Rows.Add(newRow);
            return dataTable;
        }

    }
}
