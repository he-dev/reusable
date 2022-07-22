using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Extensions;

public static class LogPropertyExtensions
{
    public static string Environment(this ILogPropertyName? _) => nameof(Environment);
    public static string Scope(this ILogPropertyName? _) => nameof(Scope);
    public static string Correlation(this ILogPropertyName? _) => nameof(Correlation);

    public static string Logger(this ILogPropertyName? _) => nameof(Logger);

    //public static string Level(this ILogPropertyName? _) => nameof(Level);
    public static string Layer(this ILogPropertyName? _) => nameof(Layer);
    public static string Category(this ILogPropertyName? _) => nameof(Category);
    public static string Tag(this ILogPropertyName? _) => nameof(Tag);
    public static string Snapshot(this ILogPropertyName? _) => nameof(Snapshot);
    public static string Snapshots(this ILogPropertyName? _) => nameof(Snapshots);
    public static string UnitOfWork(this ILogPropertyName? _) => nameof(UnitOfWork);
    public static string Exception(this ILogPropertyName? _) => nameof(Exception);
    public static string LoggerScope(this ILogPropertyName? _) => nameof(LoggerScope);
    public static string Telemetry(this ILogPropertyName? _) => nameof(Snapshots);
    
    public static string ChannelName(this ILogPropertyName? _) => $"{nameof(Channel)}.{nameof(Channel.Name)}";
    public static string ChannelOpt<T>(this ILogPropertyName? _, string? name) where T : IChannel => $"{typeof(T)}/{name}/{nameof(ChannelOpt)}";

}