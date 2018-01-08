using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        [NotNull, ContractAnnotation("rows: null => halt")]
        public static DataTable ToDataTable([NotNull, ItemNotNull] this IEnumerable<IEnumerable<string>> rows, IDictionary<string, Type> columnTypes, ITypeConverter converter)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            using (var enumerator = rows.GetEnumerator())
            {
                var dataTable = default(DataTable);
                while (enumerator.MoveNext())
                {
                    dataTable = dataTable ?? CreateDataTable(enumerator.Current, columnTypes);

                    // ReSharper disable once AssignNullToNotNullAttribute
                    var values =
                        enumerator
                            .Current
                            .Zip(dataTable.Columns.Cast<DataColumn>().Select(dc => dc.ColumnName), (value, name) => (value, type: columnTypes[name]))
                            .Select(zip => converter.Convert(zip.value, zip.type))
                            .ToArray();

                    dataTable.AddRow(values);
                }
                return dataTable ?? new DataTable();
            }
        }

        private static DataTable CreateDataTable(IEnumerable<string> row, IDictionary<string, Type> columnTypes)
        {
            var dataTable = new DataTable();

            foreach (var name in row)
            {
                dataTable.AddColumn(name, c => c.DataType = columnTypes[name]);
            }

            return dataTable;
        }
    }
}
