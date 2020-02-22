using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OneTo1;

namespace Reusable.Csv
{
    [PublicAPI]
    public static class CsvConverter
    {
        /// <summary>
        /// Creates a data-table from rows and converts columns to target types. The first row must be a header.
        /// </summary>
        /// <returns></returns>
        [NotNull, ContractAnnotation("rows: null => halt")]
        public static DataTable ToDataTable(
            [NotNull, ItemNotNull] this IEnumerable<IEnumerable<string>> rows,
            [NotNull] IEnumerable<(SoftString Name, Type Type)> columns,
            [NotNull] ITypeConverter converter)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (columns == null) throw new ArgumentNullException(nameof(columns));
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            using (var enumerator = rows.GetEnumerator())
            {
                var dataTable = default(DataTable);
                while (enumerator.MoveNext())
                {
                    if (dataTable is null)
                    {
                        dataTable = CreateDataTable(enumerator.Current, columns);
                    }
                    else
                    {
                        var values = new object[dataTable.Columns.Count];
                        var columnTypes = dataTable.Columns.Cast<DataColumn>().Select(dc => dc.DataType);
                        foreach (var (value, type, ordinal) in enumerator.Current.Zip(columnTypes, (value, type) => (value, type)).Select((zip, ordinal) => (zip.value, zip.type, ordinal)))
                        {
                            values[ordinal] = converter.ConvertOrDefault(value, type);
                        }

                        dataTable.AddRow(values);
                    }
                }
                return dataTable ?? new DataTable();
            }
        }

        private static DataTable CreateDataTable(IEnumerable<string> header, IEnumerable<(SoftString Name, Type Type)> columns)
        {
            var dataTable = new DataTable();

            foreach (var name in header)
            {
                dataTable.AddColumn(name, c => c.DataType = columns.Single(column => column.Name.Equals(name)).Type);
            }

            return dataTable;
        }       
    }
}
