using System;
using System.Runtime.CompilerServices;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Conventions;

// Timestamp | Product | Environment | Layer | Scope   | Category | Member  | Snapshot | Message
// Auto        Auto      Auto          Log     Auto      Log 
//                                                       Decision                        Description+Reason

// ReSharper disable once UnusedTypeParameter - It's all about the T that is required for linking.
public delegate void Link<in T>(ILogEntry log);

public interface ITelemetry { }

// Layers

public interface ITelemetryLayer { }

// Categories

public interface ITelemetryCategory { }

public interface ITelemetryCategoryDependency { }

public interface ITelemetryCategoryProcess { }

public interface ITelemetryCategoryLogic { }

public interface ITelemetryCategoryDecision { }

// Units

public interface ITelemetryPersistence { }

// Other

public interface ITelemetryExecution { }

// Extensions

public static class Telemetry
{
    public static Link<ITelemetry?> Collect => _ => { };
}

// Links from Telemetry to Layer
public static class TelemetryLayers
{
    public static Link<ITelemetryLayer> Presentation(this Link<ITelemetry> telemetry) => entry => telemetry(entry.Layer(nameof(Presentation)));
    public static Link<ITelemetryLayer> Application(this Link<ITelemetry> telemetry) => entry => telemetry(entry.Layer(nameof(Application)));
    public static Link<ITelemetryLayer> Business(this Link<ITelemetry> telemetry) => entry => telemetry(entry.Layer(nameof(Business)));
    public static Link<ITelemetryPersistence> Persistence(this Link<ITelemetry> telemetry) => entry => telemetry(entry.Layer(nameof(Persistence)));
}

public static class TelemetryPersistence
{
    /// <summary>
    /// Direct Attached Storage like HDD, SSD, CD, DVD, Flash.
    /// </summary>
    public static Link<ITelemetryCategory> DAS(this Link<ITelemetryPersistence> layer, string? name = default) => entry => layer(entry.Category(name ?? nameof(DAS)));

    /// <summary>
    /// Network Attached Storage.
    /// </summary>
    public static Link<ITelemetryCategory> NAS(this Link<ITelemetryPersistence> layer, string? name = default) => entry => layer(entry.Category(name ?? nameof(NAS)));

    public static Link<ITelemetryCategory> Network(this Link<ITelemetryPersistence> layer) => entry => layer(entry.Category(nameof(Network)));

    public static Link<ITelemetryCategory> Database(this Link<ITelemetryPersistence> layer) => entry => layer(entry.Category(nameof(Database)));

    public static Link<ITelemetryCategory> Cloud(this Link<ITelemetryPersistence> layer) => entry => layer(entry.Category(nameof(Cloud)));
}


public static class TelemetryCategories
{
    public static Link<ILogEntry> Decision(this Link<ITelemetryLayer> layer, string description, string because)
    {
        return entry => layer(entry.Category(nameof(Decision)).Message(description).MessageAppend($"{because}"));
    }

    public static Action<ILogEntry> Metric(this Link<ITelemetryLayer> layer, string name, double value) => entry => layer(entry.Category(nameof(Metric)).Snapshot(name, value));

    public static Action<ILogEntry> Metric(this Link<ITelemetryLayer> layer, string name, string value) => entry => layer(entry.Category(nameof(Metric)).Snapshot(name, value));

    public static Link<ITelemetryCategory> Routine(this Link<ITelemetryLayer?> layer, string name) => entry => layer(entry.Category(nameof(Routine)).Member(name));

    
    
    public static Link<ITelemetryExecution> Execution(this Link<ITelemetryLayer> layer) => entry => layer(entry.Category(nameof(Execution)));
    
    public static Action<ILogEntry> Started(this Link<ITelemetryExecution> layer, object? value = default) => entry => layer(entry.Snapshot(nameof(Started), value));
    public static Action<ILogEntry> Completed(this Link<ITelemetryExecution> layer) => entry => layer(entry.Member(nameof(Completed)));
    public static Action<ILogEntry> Cancelled(this Link<ITelemetryExecution> layer) => entry => layer(entry.Member(nameof(Cancelled)));
    public static Action<ILogEntry> Faulted(this Link<ITelemetryExecution> layer, Exception? exception = default) => entry => layer(entry.Member(nameof(Faulted)).Exception(exception));
    public static Action<ILogEntry> Auto(this Link<ITelemetryExecution> layer) => entry => layer(entry.Push(new MetaProperty.PopulateExecution()));
    
    
    public static Action<ILogEntry> Snapshot(this Link<ITelemetryCategory> layer, string name, object value) => entry => layer(entry.Snapshot(name, value));
    public static Action<ILogEntry> Argument(this Link<ITelemetryLayer> layer, string name, object value) => entry => layer(entry.Category(nameof(Argument)).Snapshot(name, value));
    public static Action<ILogEntry> Variable(this Link<ITelemetryLayer> layer, string name, object value) => entry => layer(entry.Category(nameof(Variable)).Snapshot(name, value));
    public static Action<ILogEntry> Property(this Link<ITelemetryLayer> layer, string name, object value) => entry => layer(entry.Category(nameof(Property)).Snapshot(name, value));
    public static Action<ILogEntry> Metadata(this Link<ITelemetryLayer> layer, string name, object value) => entry => layer(entry.Category(nameof(Metadata)).Snapshot(name, value));
    public static Action<ILogEntry> Metadata<T>(this Link<ITelemetryLayer> layer, T value) => entry => layer(entry.Category(nameof(Metadata)).Snapshot(typeof(T).ToPrettyString(), value));
    public static Action<ILogEntry> WorkItem(this Link<ITelemetryLayer> layer, string name, object value) => entry => layer(entry.Category(nameof(WorkItem)).Snapshot(name, value));
}

public static class TelemetryPopular
{
    public static Action<ILogEntry> Message(this Action<ILogEntry> node, string? message) => node.Then(e => e.Message(message));

    public static Action<ILogEntry> Exception(this Action<ILogEntry> node, Exception? exception) => node.Then(e => e.Exception(exception));
}

public static class TelemetryLevels
{
    public static Action<ILogEntry> Level(this Action<ILogEntry> node, LogLevel logLevel) => node.Then(e => e.Level(logLevel));

    public static Action<ILogEntry> Trace(this Action<ILogEntry> node) => node.Level();
    public static Action<ILogEntry> Debug(this Action<ILogEntry> node) => node.Level();
    public static Action<ILogEntry> Warning(this Action<ILogEntry> node) => node.Level();
    public static Action<ILogEntry> Information(this Action<ILogEntry> node) => node.Level();
    public static Action<ILogEntry> Error(this Action<ILogEntry> node) => node.Level();
    public static Action<ILogEntry> Fatal(this Action<ILogEntry> node) => node.Level();

    private static Action<ILogEntry> Level(this Action<ILogEntry> node, [CallerMemberName] string? name = null) => node.Level((LogLevel)Enum.Parse(typeof(LogLevel), name!));
}