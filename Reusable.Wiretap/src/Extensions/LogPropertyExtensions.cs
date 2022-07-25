using System;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Extensions;

public static class LogPropertyExtensions
{
    public static string Environment(this ILogPropertyName? _) => nameof(Environment);
    public static string Correlation(this ILogPropertyName? _) => nameof(Correlation);

    public static string Logger(this ILogPropertyName? _) => nameof(Logger);

    public static string Layer(this ILogPropertyName? _) => nameof(Layer);
    public static string Category(this ILogPropertyName? _) => nameof(Category);
    public static string Identifier(this ILogPropertyName? _) => nameof(Identifier);
    public static string Snapshot(this ILogPropertyName? _) => nameof(Snapshot);
    public static string Snapshots(this ILogPropertyName? _) => nameof(Snapshots);
    public static string UnitOfWork(this ILogPropertyName? _) => nameof(UnitOfWork);
    public static string Exception(this ILogPropertyName? _) => nameof(Exception);
    public static string Telemetry(this ILogPropertyName? _) => nameof(Snapshots);
    
    public static string ChannelName(this ILogPropertyName? _) => $"{nameof(Channel)}.{nameof(Channel.Name)}";
    public static string ChannelMode(this ILogPropertyName? _) => $"{nameof(Channel)}.{nameof(Channel.Mode)}";

    public static T Value<T>(this ILogProperty property)
    {
        return property.Value switch
        {
            T value => value,
            _ => throw new ArgumentException($"Property {property.Name} must by of type {typeof(T).ToPrettyString()}, but is {property.Value.GetType().ToPrettyString()}.")
        };
    }
}