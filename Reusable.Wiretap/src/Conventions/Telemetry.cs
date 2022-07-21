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

        var canCreateFile = false;
        logger.Log(Telemetry.Collect.Application().Decision(new { canCreateFile }));

        logger.Log(Telemetry.Collect.Network().Metric(new { userCount = 3 }));
    }
}

public interface ITelemetry { }

public interface ITelemetryLayer { }

public interface ITelemetryCategory { }

public interface ITelemetryPersistence { }

public interface ITelemetryUnitOfWork { }

// Extensions

public static class Telemetry
{
    public static Builder<ITelemetry> Collect => new();

    public readonly struct Builder<T>
    {
        public Builder() : this(new LogEntry()) { }

        private Builder(ILogEntry entry) => Entry = entry;

        private ILogEntry Entry { get; }

        public Builder<T> Push<TPropertyTag>(string name, object? value) where TPropertyTag : ILogPropertyTag
        {
            return this.Also(b => b.Entry.Push<TPropertyTag>(name, value));
        }

        public Builder<TNext> Next<TNext>() => new(Entry);

        public static implicit operator Action<ILogEntry>(Builder<T> builder) => entry => entry.Push(builder.Entry);
    }
}

// Links from Telemetry to Layer
public static class TelemetryLayers
{
    public static Telemetry.Builder<ITelemetryLayer> Layer(this Telemetry.Builder<ITelemetry> telemetry, [CallerMemberName] string name = default!)
    {
        return telemetry.Push<IRegularProperty>(nameof(Layer), name).Next<ITelemetryLayer>();
    }

    public static Telemetry.Builder<ITelemetryLayer> Presentation(this Telemetry.Builder<ITelemetry> telemetry) => telemetry.Layer();
    public static Telemetry.Builder<ITelemetryLayer> Application(this Telemetry.Builder<ITelemetry> telemetry) => telemetry.Layer();
    public static Telemetry.Builder<ITelemetryLayer> Business(this Telemetry.Builder<ITelemetry> telemetry) => telemetry.Layer();
    public static Telemetry.Builder<ITelemetryLayer> Persistence(this Telemetry.Builder<ITelemetry> telemetry) => telemetry.Layer();
    public static Telemetry.Builder<ITelemetryLayer> Database(this Telemetry.Builder<ITelemetry> telemetry) => telemetry.Layer();
    public static Telemetry.Builder<ITelemetryLayer> FileSystem(this Telemetry.Builder<ITelemetry> telemetry) => telemetry.Layer();
    public static Telemetry.Builder<ITelemetryLayer> Network(this Telemetry.Builder<ITelemetry> telemetry) => telemetry.Layer();
    public static Telemetry.Builder<ITelemetryLayer> Cloud(this Telemetry.Builder<ITelemetry> telemetry) => telemetry.Layer();
}

public static class TelemetryCategories
{
    public static Telemetry.Builder<ITelemetryLayer> Category(this Telemetry.Builder<ITelemetryLayer> layer, [CallerMemberName] string name = default!)
    {
        return layer.Push<IRegularProperty>(nameof(Category), name).Next<ITelemetryLayer>();
    }

    public static Action<ILogEntry> Decision(this Telemetry.Builder<ITelemetryLayer> layer, object state)
    {
        return layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state);
    }

    public static Action<ILogEntry> Metric(this Telemetry.Builder<ITelemetryLayer> layer, object state)
    {
        return layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state);
    }

    public static Telemetry.Builder<ITelemetryUnitOfWork> UnitOfWork(this Telemetry.Builder<ITelemetryLayer> layer, string tag)
    {
        return layer.Category().Push<IRegularProperty>(nameof(tag), tag).Next<ITelemetryUnitOfWork>();
    }

    public static Telemetry.Builder<ITelemetryUnitOfWork> State(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork, [CallerMemberName] string name = default!)
    {
        return unitOfWork.Push<IRegularProperty>(nameof(State), name);
    }

    public static Action<ILogEntry> Started(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork, int itemCount = 0)
    {
        return unitOfWork.State().Push<ITransientProperty>(nameof(itemCount), itemCount);
    }

    public static Action<ILogEntry> Working(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork, int itemIndex)
    {
        return unitOfWork.State().Push<ITransientProperty>(nameof(itemIndex), itemIndex);
    }

    public static Action<ILogEntry> Completed(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork)
    {
        return unitOfWork.State();
    }

    public static Action<ILogEntry> Cancelled(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork)
    {
        return unitOfWork.State();
    }

    public static Action<ILogEntry> Suspended(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork)
    {
        return unitOfWork.State();
    }

    public static Action<ILogEntry> Faulted(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork, Exception? exception = default)
    {
        return unitOfWork.State().Push<IRegularProperty>(LogProperty.Names.Exception(), exception);
    }

    /// <summary>
    /// Use this in the finally block to automatically set the result to either Completed or Faulted if there is an exception in the current scope.
    /// </summary>
    public static Action<ILogEntry> Auto(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork)
    {
        return unitOfWork.Push<IMetaProperty>(nameof(Auto), true);
    }

    public static Action<ILogEntry> Argument(this Telemetry.Builder<ITelemetryLayer> layer, object state) => layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state);
    public static Action<ILogEntry> Variable(this Telemetry.Builder<ITelemetryLayer> layer, object state) => layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state);
    public static Action<ILogEntry> Property(this Telemetry.Builder<ITelemetryLayer> layer, object state) => layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state);
    public static Action<ILogEntry> Metadata(this Telemetry.Builder<ITelemetryLayer> layer, object state) => layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state);
    public static Action<ILogEntry> WorkItem(this Telemetry.Builder<ITelemetryLayer> layer, object state) => layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state);
}