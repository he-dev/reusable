using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Utilities.Mailr.Models
{
    public class HtmlTable
    {
        public HtmlTable(IList<HtmlTableColumn> columns)
        {
            Head = new HtmlTableSection(columns);
            var row = Head.NewRow();
            foreach (var column in columns)
            {
                row[column.Name].Value = column.Name.ToString();
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

        #region JsonNet extensions

        public bool ShouldSerializeHead() => Head.Any();

        public bool ShouldSerializeBody() => Body.Any();

        public bool ShouldSerializeFoot() => Foot.Any();

        #endregion
    }

    public class HtmlTableSection : List<HtmlTableRow>
    {
        public HtmlTableSection(IEnumerable<HtmlTableColumn> columns)
        {
            Columns = columns.Select((column, ordinal) => new HtmlTableColumn
            {
                Name = column.Name,
                Ordinal = ordinal,
                Type = column.Type
            }).ToList();
        }

        [JsonIgnore]
        public IEnumerable<HtmlTableColumn> Columns { get; }
    }

    public class HtmlTableColumn
    {
        public SoftString Name { get; internal set; }

        public int Ordinal { get; internal set; }

        public Type Type { get; internal set; }

        internal static HtmlTableColumn Create<T>(SoftString name) => new HtmlTableColumn
        {
            Name = name,
            Type = typeof(T)
        };

        public static IList<HtmlTableColumn> Create(params (string Name, Type Type)[] columns)
        {
            return
                columns
                    .Select(x => new HtmlTableColumn
                    {
                        Name = x.Name,
                        Type = x.Type
                    })
                    .ToList();
        }

        public override string ToString() => $"{Name}[{Ordinal}]";
    }

    public class HtmlTableRow : IEnumerable<HtmlTableCell>
    {
        private readonly IList<HtmlTableCell> _cells;

        internal HtmlTableRow(IEnumerable<HtmlTableColumn> columns)
        {
            // All rows need to have the same length so initialize them with 'default' values.
            _cells = columns.Select(column => new HtmlTableCell(column)).ToList();
        }

        [JsonIgnore]
        [NotNull]
        public HtmlTableCell this[SoftString name] => this.Single(cell => cell.Column.Name.Equals(name));

        [JsonIgnore]
        [NotNull]
        public HtmlTableCell this[int ordinal] => this.Single(cell => cell.Column.Ordinal.Equals(ordinal));

        public IEnumerator<HtmlTableCell> GetEnumerator() => _cells.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_cells).GetEnumerator();
    }

    public class HtmlTableCell
    {
        public HtmlTableCell(HtmlTableColumn column)
        {
            Column = column;
        }

        [JsonIgnore]
        public HtmlTableColumn Column { get; }

        public IList<string> Styles { get; } = new List<string>();

        public object Value { get; set; }

        #region JsonNet extensions

        public bool ShouldSerializeStyles() => Styles.Any();

        #endregion
    }

    public static class HtmlTableSectionExtensions
    {
        [NotNull]
        public static HtmlTableRow NewRow(this HtmlTableSection section)
        {
            var newRow = new HtmlTableRow(section.Columns);
            section.Add(newRow);
            return newRow;
        }

        public static void Add(this HtmlTableSection table, IEnumerable<object> values)
        {
            var newRow = table.NewRow();
            foreach (var (column, value) in table.Columns.Zip(values, (column, value) => (column, value)))
            {
                newRow[column.Name].Value = value;
            }
        }

        public static void Add(this HtmlTableSection table, params object[] values) => table.Add((IEnumerable<object>)values);
    }

    public static class HtmlTableRowExtensions
    {
        [CanBeNull]
        public static T ValueOrDefault<T>(this HtmlTableRow row, SoftString name) => row[name] is T value ? value : default;

        [CanBeNull]
        public static T ValueOrDefault<T>(this HtmlTableRow row, int ordinal) => row[ordinal] is T value ? value : default;

        [NotNull]
        public static HtmlTableRow Update(this HtmlTableRow row, string column, object value, params string[] styles)
        {
            row[column].Value = value;
            foreach (var style in styles ?? Enumerable.Empty<string>())
            {
                row[column].Styles.Add(style);
            }

            return row;
        }
    }
}