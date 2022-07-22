using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.ChannelFilters;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Nodes.Scopeable;

namespace Reusable.Wiretap.Extensions;

public static class LogEntryExtensions
{
    public static bool ContainsProperty(this ILogEntry entry, string name) => entry.TryPeek(name, out _);

    public static ILogEntry Push<T>(this ILogEntry entry, string name, object? value) where T : ILogPropertyTag
    {
        return
            value is { }
                ? entry.Push(new LogProperty<T>(name, value))
                : entry;
    }

    public static T GetValueOrDefault<T>(this ILogEntry entry, string name, T defaultValue)
    {
        return entry[name].Value is T result ? result : defaultValue;
    }

    public static ILogEntry Layer(this ILogEntry entry, string value) => entry.Push<IRegularProperty>(nameof(Layer), value);
    public static ILogEntry Category(this ILogEntry entry, string value) => entry.Push<IRegularProperty>(nameof(Category), value);
    public static ILogEntry Tag(this ILogEntry entry, string value) => entry.Push<IRegularProperty>(nameof(Tag), value);
    public static ILogEntry Snapshot(this ILogEntry entry, object? value) => entry.Push<ITransientProperty>(nameof(Snapshot), value);
    public static ILogEntry Message(this ILogEntry entry, string? value) => entry.Push<IRegularProperty>(nameof(Message), value);
    public static ILogEntry Exception(this ILogEntry entry, Exception? value) => entry.Push<IRegularProperty>(nameof(Exception), value);

    public static ILogEntry Force(this ILogEntry entry) => entry.Push<IMetaProperty>(nameof(ScopeBuffer), ScopeBuffer.Mode.Force);
    public static ILogEntry Defer(this ILogEntry entry) => entry.Push<IMetaProperty>(nameof(ScopeBuffer), ScopeBuffer.Mode.Defer);
    
    public static ILogEntry OptIn<T>(this ILogEntry entry, string? name = default) where T : IChannel
    {
        return entry.Push<IMetaProperty>(LogProperty.Names.ChannelOpt<T>(name), ChannelOpt.Mode.OptIn);
    }

    public static ILogEntry OptOut<T>(this ILogEntry entry, string? name = default) where T : IChannel
    {
        return entry.Push<IMetaProperty>(LogProperty.Names.ChannelOpt<T>(name), ChannelOpt.Mode.OptOut);
    }

    public static IEnumerable<ILogProperty> WhereTag<T>(this ILogEntry entry) where T : ILogPropertyTag
    {
        return entry.Where(property => property is ILogProperty<T>);
    }

    /// <summary>
    /// Pushed one entry over the other.
    /// </summary>
    public static ILogEntry Push(this ILogEntry entry, ILogEntry other)
    {
        foreach (var property in other)
        {
            entry.Push(property);
        }

        return entry;
    }

    //public static ILogEntry Applied<T>(ILogEntry entry, T node) where T : ILoggerNode => entry.Push<IMetaProperty>(LogProperty.Names.Telemetry(), $"{nameof(Applied)}: {node}");
    
    //public static ILogEntry Skipped<T>(ILogEntry entry, T node) where T : ILoggerNode => entry.Push<IMetaProperty>(LogProperty.Names.Telemetry(), $"{nameof(Skipped)}: {node}");


    public static DataTable ToDataTable(this IEnumerable<ILogEntry> entries, Action<DataRow>? dataRowAction = default)
    {
        var dataTable = new DataTable();
        foreach (var entry in entries)
        {
            var dataRow = dataTable.NewRow();
            foreach (var item in entry.WhereTag<IRegularProperty>())
            {
                Add(dataTable.Columns, item.Name, item.Value.GetType());
                dataRow[item.Name] = item.Value;
            }

            dataTable.Rows.Add(dataRow.Also(dataRowAction));
        }

        return dataTable;
    }

    // Adds data-column it if does not exists.
    private static DataColumnCollection Add(this DataColumnCollection columns, string name, Type dataType)
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