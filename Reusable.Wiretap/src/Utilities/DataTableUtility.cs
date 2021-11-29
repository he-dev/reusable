using System;
using System.Collections.Generic;
using System.Data;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Nodes;

namespace Reusable.Wiretap.Utilities
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
            return columns.Also(c =>
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