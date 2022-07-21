using System;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Conventions;

/*
    Telemetry.Collect
    ---
    
                                                1        2          3           4          5                                                          
    Timestamp | Product | Environment | Scope | Layer  | Category | Label/Tag | Snapshot | Message
    ---------   -------   -----------   -----   -----    --------   ---------   --------   -------
    Auto        Auto      Auto          Auto    Log      Log 
                                                         Decision                        Description+Reason                                                         
                                                 
*/

internal static class Examples
{
    public static void Main()
    {
        var logger = default(ILogger)!;

        //                           Layer         Category Tag      Snapshot
        logger.Log(Telemetry.Collect.Application().WorkItem(new { name = "value" }));

        //                           Layer         Category   Tag     Snapshot  
        logger.Log(Telemetry.Collect.Application().UnitOfWork("name").Started());
        logger.Log(Telemetry.Collect.Application().UnitOfWork("name").Started());
        logger.Log(Telemetry.Collect.Application().UnitOfWork("name").Cancelled());
        logger.Log(Telemetry.Collect.Application().UnitOfWork("name").Auto());

        //                           Layer         Category Tag
        logger.Log(Telemetry.Collect.Application().Decision("tag", "description"));

        var canCreateFile = false;
        logger.Log(Telemetry.Collect.Application().Decision(new { canCreateFile }));


        logger.Log(Telemetry.Collect.Network().Metric("name", "value"));
    }
}

// ReSharper disable once UnusedTypeParameter - It's all about the T that is required for linking.
public delegate void Link<in T>(ILogEntry log);

public delegate void Link2<in T>(params ILogProperty[] properties);

public interface ITelemetry { }

public interface ITelemetryLayer { }

public interface ITelemetryCategory { }

public interface ITelemetryPersistence { }

public interface ITelemetryUnitOfWork { }

// Extensions

public static class Telemetry
{
    public static Link<ITelemetry?> Collect => _ => { };

    public static Builder<ITelemetry> Collect2 => new();

    public readonly struct Builder<T>
    {
        public Builder() : this(new LogEntry()) { }

        private Builder(ILogEntry entry) => Entry = entry;

        private ILogEntry Entry { get; }

        public Builder<T> Push<TPropertyTag>(string name, object value) where TPropertyTag : ILogPropertyTag
        {
            return this.Also(b => b.Entry.Push<TPropertyTag>(name, value));
        }

        public Builder<TNext> Next<TNext>() => new(Entry);

        //public void Log(ILogEntry entry) => entry.Push(Entry);
        
        public static implicit operator Action<ILogEntry>(Builder<T> builder) => entry => entry.Push(builder.Entry);
    }
}

// Links from Telemetry to Layer
public static class TelemetryLayers
{
    public static Telemetry.Builder<ITelemetryLayer> Presentation(this Telemetry.Builder<ITelemetry> telemetry) => telemetry.Push<IRegularProperty>("Layer", nameof(Presentation)).Next<ITelemetryLayer>();

    public static Link<ITelemetryLayer> Presentation(this Link<ITelemetry> telemetry) => entry => telemetry(entry.Layer(nameof(Presentation)));
    public static Link<ITelemetryLayer> Application(this Link<ITelemetry> telemetry) => entry => telemetry(entry.Layer(nameof(Application)));
    public static Link<ITelemetryLayer> Business(this Link<ITelemetry> telemetry) => entry => telemetry(entry.Layer(nameof(Business)));
    public static Link<ITelemetryLayer> Persistence(this Link<ITelemetry> telemetry) => entry => telemetry(entry.Layer(nameof(Persistence)));
    public static Link<ITelemetryLayer> Database(this Link<ITelemetry> telemetry) => entry => telemetry(entry.Layer(nameof(Database)));
    public static Link<ITelemetryLayer> FileSystem(this Link<ITelemetry> telemetry) => entry => telemetry(entry.Layer(nameof(Database)));
    public static Link<ITelemetryLayer> Network(this Link<ITelemetry> layer) => entry => layer(entry.Category(nameof(Network)));
    public static Link<ITelemetryLayer> Cloud(this Link<ITelemetry> layer) => entry => layer(entry.Category(nameof(Cloud)));
}

public static class TelemetryCategories
{
    public static Action<ILogEntry> Decision(this Link<ITelemetryLayer> layer, string name, string description, string? reason = default)
    {
        return entry => { layer(entry.Category(nameof(Decision)).Tag(name).Snapshot(new { description, reason })); };
    }

    public static Action<ILogEntry> Decision(this Link<ITelemetryLayer> layer, object nameValuePair)
    {
        return entry => { layer(entry.Category(nameof(Decision)).Push<ITransientProperty>(LogProperty.Names.NameValuePair(), nameValuePair)); };
    }

    public static Action<ILogEntry> Metric(this Link<ITelemetryLayer> layer, string name, object value)
    {
        return entry => { layer(entry.Category(nameof(Metric)).Tag(name).Snapshot(value)); };
    }

    public static Action<ILogEntry> Metric(this Link<ITelemetryLayer> layer, object nameValuePair)
    {
        return entry => { layer(entry.Category(nameof(Metric)).Push<ITransientProperty>(LogProperty.Names.NameValuePair(), nameValuePair)); };
    }
    
    public static Action<ILogEntry> Metric(this Telemetry.Builder<ITelemetryLayer> layer, object nameValuePair)
    {
        return layer.Push<IRegularProperty>("Category", nameof(Metric)).Push<ITransientProperty>(LogProperty.Names.NameValuePair(), nameValuePair);
    }

    public static Link<ITelemetryUnitOfWork> UnitOfWork(this Link<ITelemetryLayer> layer, string name)
    {
        return entry =>
        {
            layer(entry
                .Category(nameof(UnitOfWork))
                .Tag(name)
            );
        };
    }

    //public static Action<ILogEntry> Started(this Link<ITelemetryUnitOfWork> layer) => entry => layer(entry.Snapshot(new { state = nameof(Started) }));
    public static Action<ILogEntry> Started(this Link2<ITelemetryUnitOfWork> layer, int itemCount = 0)
    {
        return entry =>
        {
            // layer(entry
            //     .Push<ITransientProperty>(nameof(UnitOfWork), nameof(Started))
            //     .Push<ITransientProperty>(nameof(itemCount), itemCount)
            // );
            layer(
                new LogProperty<ITransientProperty>(nameof(UnitOfWork), nameof(Started)),
                new LogProperty<ITransientProperty>(nameof(itemCount), itemCount)
            );
        };
    }

    public static Action<ILogEntry> Working(this Link<ITelemetryUnitOfWork> layer, int itemNumber)
    {
        return entry =>
        {
            layer(entry
                .Push<ITransientProperty>(nameof(UnitOfWork), nameof(Working))
                .Push<ITransientProperty>(nameof(itemNumber), itemNumber)
            );
        };
    }

    public static Action<ILogEntry> Completed(this Link<ITelemetryUnitOfWork> layer) => entry => layer(entry.Snapshot(new { state = nameof(Completed) }));
    public static Action<ILogEntry> Cancelled(this Link<ITelemetryUnitOfWork> layer) => entry => layer(entry.Snapshot(new { state = nameof(Cancelled) }));
    public static Action<ILogEntry> Suspended(this Link<ITelemetryUnitOfWork> layer) => entry => layer(entry.Snapshot(new { state = nameof(Suspended) }));

    public static Action<ILogEntry> Faulted(this Link<ITelemetryUnitOfWork> layer, Exception? exception = default)
    {
        return entry => { layer(entry.Snapshot(new { state = nameof(Faulted) }).Exception(exception)); };
    }

    /// <summary>
    /// Use this in the finally block to automatically set the result to either Completed or Faulted if there is an exception in the current scope.
    /// </summary>
    public static Action<ILogEntry> Auto(this Link<ITelemetryUnitOfWork> layer)
    {
        return entry => { layer(entry); };
    }

    public static Action<ILogEntry> Argument(this Link<ITelemetryLayer> layer, object nameValuePair) => entry => layer(entry.Category(nameof(Argument)).NameValuePair(nameValuePair));
    public static Action<ILogEntry> Variable(this Link<ITelemetryLayer> layer, object nameValuePair) => entry => layer(entry.Category(nameof(Variable)).NameValuePair(nameValuePair));
    public static Action<ILogEntry> Property(this Link<ITelemetryLayer> layer, object nameValuePair) => entry => layer(entry.Category(nameof(Property)).NameValuePair(nameValuePair));
    public static Action<ILogEntry> Metadata(this Link<ITelemetryLayer> layer, object nameValuePair) => entry => layer(entry.Category(nameof(Metadata)).NameValuePair(nameValuePair));
    public static Action<ILogEntry> WorkItem(this Link<ITelemetryLayer> layer, object nameValuePair) => entry => layer(entry.Category(nameof(WorkItem)).NameValuePair(nameValuePair));
}

// public static class TelemetryLevels
// {
//     public static Action<ILogEntry> Level(this Action<ILogEntry> node, LogLevel logLevel) => node.Then(e => e.Level(logLevel));
//
//     public static Action<ILogEntry> Trace(this Action<ILogEntry> node) => node.ParseLogLevel();
//     public static Action<ILogEntry> Debug(this Action<ILogEntry> node) => node.ParseLogLevel();
//     public static Action<ILogEntry> Warning(this Action<ILogEntry> node) => node.ParseLogLevel();
//     public static Action<ILogEntry> Information(this Action<ILogEntry> node) => node.ParseLogLevel();
//     public static Action<ILogEntry> Error(this Action<ILogEntry> node) => node.ParseLogLevel();
//     public static Action<ILogEntry> Fatal(this Action<ILogEntry> node) => node.ParseLogLevel();
//
//     private static Action<ILogEntry> ParseLogLevel(this Action<ILogEntry> node, [CallerMemberName] string? name = null) => node.Level((LogLevel)Enum.Parse(typeof(LogLevel), name!));
// }