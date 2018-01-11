using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Custom;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Converters;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable
{
    public static class CsvConverter
    {
        /// <summary>
        /// Creates a data-table from rows and converts columns to target types.
        /// The first row must be a header.
        /// In order to use it with SqlBulkInsert columns must be ordered.
        /// </summary>
        /// <returns></returns>
        [NotNull, ContractAnnotation("rows: null => halt")]
        public static DataTable ToDataTable(
            [NotNull, ItemNotNull] this IEnumerable<IEnumerable<string>> rows,
            [NotNull] IEnumerable<(string Name, Type Type)> columns,
            [NotNull] ITypeConverter converter)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            if (columns == null) throw new ArgumentNullException(nameof(columns));
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            var columnMap = default(IDictionary<int, int>);

            using (var enumerator = rows.GetEnumerator())
            {
                var dataTable = default(DataTable);
                while (enumerator.MoveNext())
                {
                    if (dataTable is default)
                    {
                        dataTable = CreateDataTable(columns);
                        columnMap = CreateColumnMap(enumerator.Current, columns.Select(x => x.Name));
                    }
                    else
                    {
                        var values = new object[columns.Count()];

                        foreach (var item in enumerator.Current.Select((value, ordinal) => (value, ordinal)))
                        {
                            var columnOrdinal = columnMap[item.ordinal];
                            var column = columns.ElementAt(columnOrdinal);
                            values[columnOrdinal] = converter.Convert(item.value, column.Type);
                        }

                        dataTable.AddRow(values);
                    }
                }
                return dataTable ?? new DataTable();
            }
        }

        private static DataTable CreateDataTable(IEnumerable<(string Name, Type Type)> columns)
        {
            var dataTable = new DataTable();

            foreach (var (name, type) in columns)
            {
                dataTable.AddColumn(name, c => c.DataType = type);
            }

            return dataTable;
        }

        // There might be fewer columns in a csv then the target table has so we need to calculate their offsets.
        // <csv-column-ordinal, sql-column-ordinal>
        private static IDictionary<int, int> CreateColumnMap(IEnumerable<string> csvNames, IEnumerable<string> sqlNames)
        {
            var csvMap = csvNames.Select((name, ordinal) => (name, ordinal)).ToList();
            var sqlMap = sqlNames.Select((name, ordinal) => (name, ordinal)).ToList();
            return
                csvMap
                    .ToDictionary(
                        x => x.ordinal,
                        x => sqlMap.Single(y => y.name.Equals(x.name, StringComparison.OrdinalIgnoreCase)).ordinal);
        }
    }
}
