using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

public static class LogEntryItems
{
    public static T? GetItem<T>(this LogEntry entry, [CallerMemberName] string name = "") => entry.TryGetValue(name, out var value) && value is T result ? result : default;

    public static T GetItemOrDefault<T>(this LogEntry entry, string key, T defaultValue)
    {
        return
            entry.TryGetValue(key, out var value) && value is T result
                ? result
                : defaultValue;
    }

    public static LogEntry SetItem(this LogEntry entry, object? value, [CallerMemberName] string name = "") => entry.SetItem(name, value);

    public static LogEntry Module(this LogEntry entry, IModule module) => entry.SetItem(module);
    public static IModule? Module(this LogEntry entry) => entry.GetItem<IModule>();

    public static LogEntry Elapsed(this LogEntry entry, double value) => entry.SetItem(value);

    public static object UniqueId(this LogEntry entry) => entry.GetItem<object>() ?? new InvalidOperationException("The unique-id doesn't seem to be initialized yet.");
    public static LogEntry UniqueId(this LogEntry entry, object? value) => entry.SetItem(value);

    public static DateTime? Timestamp(this LogEntry entry) => entry.GetItem<DateTime>();
    public static LogEntry Timestamp(this LogEntry entry, DateTime value) => entry.SetItem(value);

    public static IImmutableDictionary<string, object?> Details(this LogEntry entry) => entry.GetItem<IImmutableDictionary<string, object?>>() ?? throw new InvalidOperationException("Details don't seem to be initialized yet.");
    public static LogEntry Timestamp_(this LogEntry entry, DateTime value) => entry.SetItem(value);
}