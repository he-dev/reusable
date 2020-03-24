using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Utilities
{
    public static class DataTableUtility
    {
        public static DataTable ToDataTable(this IEnumerable<ILogEntry> entries, Action<DataRow>? processDataRow = default)
        {
            var dataTable = new DataTable();
            foreach (var entry in entries)
            {
                var dataRow = dataTable.NewRow();
                foreach (var item in entry.Where(LogProperty.CanProcess.With<Echo>()))
                {
                    dataTable.Columns.SoftAdd(item.Name, item.Value.GetType());
                    dataRow[item.Name] = item.Value;
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        // Adds data-column it if does not exists.
        public static DataColumnCollection SoftAdd(this DataColumnCollection columns, string name, Type dataType)
        {
            return columns.Pipe(c =>
            {
                if (!c.Contains(name))
                {
                    c.Add(name, dataType);
                }
            });
        }
    }


    // public static class JsonUtility
    // {
    //     public static JToken Parse(this DataRow dataRow, string sourceColumn)
    //     {
    //         return JToken.Parse(dataRow.Field<string>(sourceColumn));
    //     }
    // }
}