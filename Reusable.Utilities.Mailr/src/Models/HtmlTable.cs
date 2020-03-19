using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.Utilities.Mailr.Models
{
    public class HtmlTable
    {
        public HtmlTable(IEnumerable<HtmlTableColumn> columns)
        {
            columns = columns.ToList();
            Head = new HtmlTableSection(columns);
            var row = Head.AddRow();
            foreach (var column in columns)
            {
                row[column.Name].Value = column.Name.ToString();
            }

            Body = new HtmlTableSection(columns);
            Foot = new HtmlTableSection(columns);
        }

        public HtmlTable(IEnumerable<(string Name, Type Type)> columns) : this(HtmlTableColumn.Create(columns)) { }
        
        public HtmlTable(params (string Name, Type Type)[] columns) : this(HtmlTableColumn.Create(columns.AsEnumerable())) { }

        public HtmlTableSection Head { get; }

        public HtmlTableSection Body { get; }

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
        public SoftString? Name { get; internal set; }

        public int Ordinal { get; internal set; }

        public Type? Type { get; internal set; }

        internal static HtmlTableColumn Create<T>(SoftString name) => new HtmlTableColumn
        {
            Name = name,
            Type = typeof(T)
        };

        public static IEnumerable<HtmlTableColumn> Create(IEnumerable<(string Name, Type Type)> columns)
        {
            return
                from column in columns
                select new HtmlTableColumn
                {
                    Name = column.Name,
                    Type = column.Type
                };
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

        [Obsolete("Use 'Tags'.")]
        public ISet<string> Styles { get; } = new HashSet<string>(SoftString.Comparer);

        public ISet<string> Tags { get; } = new HashSet<string>(SoftString.Comparer);

        public object Value { get; set; }

        #region JsonNet extensions

        public bool ShouldSerializeStyles() => Styles.Any();

        public bool ShouldSerializeTags() => Tags.Any();

        #endregion
    }

    public static class HtmlTableSectionExtensions
    {
        public static HtmlTableRow AddRow(this HtmlTableSection section)
        {
            return new HtmlTableRow(section.Columns).Pipe(section.Add);
        }

        public static void Add(this HtmlTableSection table, IEnumerable<object> values)
        {
            var newRow = table.AddRow();
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

        public static HtmlTableRow Set(this HtmlTableRow row, string column, object value, params string[] tags)
        {
            row[column].Value = value;
            row[column].Tags.UnionWith(tags);
            return row;
        }
        
        public static HtmlTableCell Column(this HtmlTableRow row, string column) => row[column];
    }
}