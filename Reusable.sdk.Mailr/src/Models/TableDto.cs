using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.UI.WebControls;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.sdk.Mailr.Models
{
    public class TableDto
    {
        public IList<IList<TableCell>> Head { get; set; }

        public IList<IList<TableCell>> Body { get; set; }

        public IList<IList<TableCell>> Foot { get; set; }
    }

    public class TableCellDto
    {
        public IList<string> Styles { get; set; }

        public object Data { get; set; }
    }

    public class HtmlTable
    {
        public HtmlTable(IEnumerable<HtmlTableColumn> columns, bool areHeaders = true)
        {
            //var columnList = columns.ToList();

            Head = new HtmlTableSection(columns);
            if (areHeaders)
            {
                var row = Head.NewRow();
                foreach (var column in columns)
                {
                    row[column.Name] = column.Name.ToString();
                }
            }

            Body = new HtmlTableSection(columns);
            Foot = new HtmlTableSection(columns);
        }

        [NotNull]
        public HtmlTableSection Head { get; }

        [NotNull]
        public HtmlTableSection Body { get; }

        [NotNull]
        public HtmlTableSection Foot { get; }

        [NotNull]
        public IDictionary<string, IEnumerable<IEnumerable<object>>> Dump()
        {
            return new Dictionary<string, IEnumerable<IEnumerable<object>>>
            {
                [nameof(Head)] = Head.Dump(),
                [nameof(Body)] = Body.Dump(),
                [nameof(Foot)] = Foot.Dump(),
            };
        }
    }

    public class HtmlTableSection
    {
        private readonly IDictionary<SoftString, HtmlTableColumn> _columnByName;
        private readonly IDictionary<int, HtmlTableColumn> _columnByOrdinal;

        private readonly List<HtmlTableRow> _rows = new List<HtmlTableRow>();

        public HtmlTableSection(IEnumerable<HtmlTableColumn> columns)
        {
            columns = columns.Select((column, ordinal) => new HtmlTableColumn
            {
                Name = column.Name,
                Ordinal = ordinal,
                Type = column.Type
            }).ToList();
            _columnByName = columns.ToDictionary(x => x.Name);
            _columnByOrdinal = columns.ToDictionary(x => x.Ordinal);
        }     

        internal IEnumerable<HtmlTableColumn> Columns => _columnByName.Values;
     
        [NotNull]
        public HtmlTableRow NewRow()
        {
            var newRow = new HtmlTableRow
            (
                _columnByName.Values,
                name => _columnByName.GetItemSafely(name),
                ordinal => _columnByOrdinal.GetItemSafely(ordinal)
            );
            _rows.Add(newRow);
            return newRow;
        }

        [NotNull, ItemNotNull]
        public IEnumerable<IEnumerable<object>> Dump() => _rows.Select(row => row.Dump());
    }

    public static class TableDtoExtensions
    {
        public static void Add(this HtmlTableSection table, IEnumerable<object> values)
        {
            var newRow = table.NewRow();
            foreach (var (column, value) in table.Columns.Zip(values, (column, value) => (column, value)))
            {
                newRow[column.Name] = value;
            }
        }

        public static void Add(this HtmlTableSection table, params object[] values) => table.Add((IEnumerable<object>)values);
    }

    public class HtmlTableColumn : IComparable<HtmlTableColumn>
    {
        public static readonly IComparer<HtmlTableColumn> Comparer = ComparerFactory<HtmlTableColumn>.Create(x => x.Ordinal);       

        public SoftString Name { get; internal set; }

        public int Ordinal { get; internal set; }

        public Type Type { get; internal set; }

        internal static HtmlTableColumn Create<T>(SoftString name) => new HtmlTableColumn
        {
            Name = name,            
            Type = typeof(T)
        };

        public int CompareTo(HtmlTableColumn other) => Comparer.Compare(this, other);

        public override string ToString() => $"{Name}[{Ordinal}]";
    }

    public class HtmlTableRow
    {
        private readonly IDictionary<HtmlTableColumn, object> _data;
        private readonly Func<SoftString, HtmlTableColumn> _getColumnByName;
        private readonly Func<int, HtmlTableColumn> _getColumnByOrdinal;

        internal HtmlTableRow(IEnumerable<HtmlTableColumn> columns, Func<SoftString, HtmlTableColumn> getColumnByName, Func<int, HtmlTableColumn> getColumnByOrdinal)
        {
            // All rows need to have the same length so initialize them with 'default' values.
            _data = new SortedDictionary<HtmlTableColumn, object>(columns.ToDictionary(x => x, _ => default(object)));
            _getColumnByName = getColumnByName;
            _getColumnByOrdinal = getColumnByOrdinal;
        }

        [CanBeNull]
        public object this[SoftString name]
        {
            get => _data.GetItemSafely(_getColumnByName(name));
            set => SetValue(_getColumnByName(name), value);
        }

        [CanBeNull]
        public object this[int ordinal]
        {
            get => _data.GetItemSafely(_getColumnByOrdinal(ordinal));
            set => SetValue(_getColumnByOrdinal(ordinal), value);
        }

        private void SetValue(HtmlTableColumn column, object value)
        {
            if (!(value is null) && value.GetType() != column.Type)
            {
                throw DynamicException.Create(
                    $"{column.Name.ToString()}Type",
                    $"The specified value has an invalid type for this column. Expected '{column.Type.Name}' but found '{value.GetType().ToPrettyString()}'."
                );
            }

            _data[column] = value;
        }

        [NotNull, ItemCanBeNull]
        public IEnumerable<object> Dump() => _data.Values;
    }

    public static class RowDtoExtensions
    {
        public static T Value<T>(this HtmlTableRow row, SoftString name) => row[name] is T value ? value : default;

        public static T Value<T>(this HtmlTableRow row, int ordinal) => row[ordinal] is T value ? value : default;
    }
    
}