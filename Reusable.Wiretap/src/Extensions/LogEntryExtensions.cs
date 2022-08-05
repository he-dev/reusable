using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Marbles;
using Reusable.Marbles.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Middleware;

namespace Reusable.Wiretap.Extensions;

//[PublicAPI]
public static class LogEntryExtensions
{
    public static bool ContainsProperty(this ILogEntry entry, string name) => entry.TryPeek(name, out _);

    public static ILogEntry Push<T>(this ILogEntry entry, string name, object? value) where T : ILogPropertyGroup
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

    public static ILogEntry Environment(this ILogEntry entry, object value) => entry.Push<IRegularProperty>(nameof(Environment), value);
    public static ILogEntry Logger(this ILogEntry entry, object value) => entry.Push<IRegularProperty>(nameof(Logger), value);
    public static ILogEntry Timestamp(this ILogEntry entry, DateTime value) => entry.Push<IRegularProperty>(nameof(Timestamp), value);
    public static ILogEntry Correlation(this ILogEntry entry, object value) => entry.Push<ITransientProperty>(nameof(Correlation), value);
    public static ILogEntry Layer(this ILogEntry entry, string value) => entry.Push<IRegularProperty>(nameof(Layer), value);
    public static ILogEntry Category(this ILogEntry entry, string value) => entry.Push<IRegularProperty>(nameof(Category), value);
    public static ILogEntry Identifier(this ILogEntry entry, string value) => entry.Push<IRegularProperty>(nameof(Identifier), value);

    public static ILogEntry Snapshot(this ILogEntry entry, object? value)
    {
        return value switch
        {
            string => entry.Push<IRegularProperty>(nameof(Snapshot), value),
            _ => entry.Push<ITransientProperty>(nameof(Snapshot), value),
        };
    }

    public static ILogEntry Elapsed(this ILogEntry entry, object value) => entry.Push<ITransientProperty>(nameof(Elapsed), value);
    public static ILogEntry Message(this ILogEntry entry, string? value) => entry.Push<IRegularProperty>(nameof(Message), value);
    public static ILogEntry Exception(this ILogEntry entry, Exception? value) => entry.Push<IRegularProperty>(nameof(Exception), value);

    public static ILogEntry Force(this ILogEntry entry) => entry.Push<IMetaProperty>(nameof(UnitOfWorkBuffer), UnitOfWorkBuffer.Mode.Force);
    public static ILogEntry Defer(this ILogEntry entry) => entry.Push<IMetaProperty>(nameof(UnitOfWorkBuffer), UnitOfWorkBuffer.Mode.Defer);

    public static ILogEntry OptIn(this ILogEntry entry, string name)
    {
        return
            entry
                .Push<IMetaProperty>(LogProperty.Names.ChannelName(), name)
                .Push<IMetaProperty>(LogProperty.Names.ChannelMode(), Channel.Mode.OptIn);
    }

    public static ILogEntry OptIn<T>(this ILogEntry entry) where T : IChannel => entry.OptIn(typeof(T).ToPrettyString());

    public static ILogEntry OptOut(this ILogEntry entry, string name)
    {
        return
            entry
                .Push<IMetaProperty>(LogProperty.Names.ChannelName(), name)
                .Push<IMetaProperty>(LogProperty.Names.ChannelMode(), Channel.Mode.OptOut);
    }

    public static ILogEntry OptOut<T>(this ILogEntry entry) => entry.OptOut(typeof(T).ToPrettyString());

    public static IEnumerable<ILogProperty> OfType<T>(this ILogEntry entry) where T : ILogPropertyGroup
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
            foreach (var item in entry.OfType<IRegularProperty>())
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