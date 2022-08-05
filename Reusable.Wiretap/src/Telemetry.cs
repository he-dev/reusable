using System;
using System.Runtime.CompilerServices;
using Reusable.Marbles;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap;

/*
    Telemetry.Collect
    ---
    
                                                1        2          3           4          5                                                          
    Timestamp | Product | Environment | Correlation | Layer  | Category | Identifier   | Snapshot | Message
    ---------   -------   -----------   -----------   -----    --------   ------------   --------   -------
    Auto        Auto      Auto          Auto          Log      Log 
                                                               Decision                        Description+Reason      
                                                               UnitOfWork Started
                                                         
    Layer
    - Category
      - Item
      - Snapshot
      - Message                                                  
                                                 
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
    public const string Template = "[{Timestamp:HH:mm:ss:fff}] | {Logger} | {Correlation} | {Layer}/{Category}/{Identifier}={Snapshot} ({Elapsed})| {Message} | {Exception}";
    
    public static Builder<ITelemetry> Collect => new(LogEntry.Empty());

    public record Builder<T>(ILogEntry Entry)
    {
        public Builder<T> Push<TGroup>(string name, object? value) where TGroup : ILogPropertyGroup
        {
            return this.Also(b => b.Entry.Push<TGroup>(name, value));
        }

        public Builder<TNext> Next<TNext>() => new(Entry);

        // Not using an implicit operator as it would hide invalid use cases where the chain isn't complete yet.
        public Action<ILogEntry> Build() => entry => entry.Push(Entry);
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
        return layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state).Build();
    }

    public static Action<ILogEntry> Metric(this Telemetry.Builder<ITelemetryLayer> layer, object state)
    {
        return layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state).Build();
    }

    public static Action<ILogEntry> Metric(this Telemetry.Builder<ITelemetryLayer> layer, int itemCount, int itemIndex)
    {
        return layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), new
        {
            progress = new
            {
                itemCount,
                itemIndex,
                percentage = Math.Round(itemIndex / (double)itemCount, 2)
            }
        }).Build();
    }

    internal static Telemetry.Builder<ITelemetryUnitOfWork> UnitOfWork(this Telemetry.Builder<ITelemetryLayer> layer, string? name = default)
    {
        return layer.Category().Push<IRegularProperty>(LogProperty.Names.Identifier(), name).Next<ITelemetryUnitOfWork>();
    }

    public static Telemetry.Builder<ITelemetryUnitOfWork> State(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork, [CallerMemberName] string name = default!)
    {
        return unitOfWork.Push<ITransientProperty>(LogProperty.Names.Snapshot(), new { flow = name });
    }

    internal static Action<ILogEntry> Started(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork)
    {
        return unitOfWork.State().Build();
    }

    internal static Action<ILogEntry> Completed(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork)
    {
        return unitOfWork.State().Build();
    }

    internal static Action<ILogEntry> Cancelled(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork)
    {
        return unitOfWork.State().Build();
    }

    internal static Action<ILogEntry> Faulted(this Telemetry.Builder<ITelemetryUnitOfWork> unitOfWork, Exception? exception = default)
    {
        return unitOfWork.State().Push<IRegularProperty>(LogProperty.Names.Exception(), exception).Build();
    }

    public static Action<ILogEntry> Argument(this Telemetry.Builder<ITelemetryLayer> layer, object state) => layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state).Build();
    public static Action<ILogEntry> Variable(this Telemetry.Builder<ITelemetryLayer> layer, object state) => layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state).Build();
    public static Action<ILogEntry> Property(this Telemetry.Builder<ITelemetryLayer> layer, object state) => layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state).Build();
    public static Action<ILogEntry> Metadata(this Telemetry.Builder<ITelemetryLayer> layer, object state) => layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state).Build();
    public static Action<ILogEntry> WorkItem(this Telemetry.Builder<ITelemetryLayer> layer, object state) => layer.Category().Push<ITransientProperty>(LogProperty.Names.Snapshots(), state).Build();
}