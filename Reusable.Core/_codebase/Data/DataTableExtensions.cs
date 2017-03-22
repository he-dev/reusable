using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Data
{
    public static class DataTableExtensions
    {
        public static DataTable AddRow(this DataTable dt, params object[] values)
        {
            var row = dt.NewRow();
            row.ItemArray = values;
            dt.Rows.Add(row);
            return dt;
        }

        public static DataTable AddColumn(this DataTable dt, string name, Action<DataColumn> customizeColumn = null)
        {
            var column = dt.Columns.Add(name);
            customizeColumn?.Invoke(column);
            return dt;
        }
    }
}
