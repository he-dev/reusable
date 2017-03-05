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
    }
}
