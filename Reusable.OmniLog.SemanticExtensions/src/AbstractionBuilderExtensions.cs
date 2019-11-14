using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Abstractions.Data.LogPropertyActions;

namespace Reusable.OmniLog.SemanticExtensions
{
    /*
     
     Abstraction
        > Layer (Business | Infrastructure | ...) 
        > Category (Data | Action) 
        > Snapshot (Object | Property | ...)
     
    logger.Log(Abstraction.Layer.Business().Data().Object(new { customer }));
    logger.Log(Abstraction.Layer.Infrastructure().Data().Variable(new { customer }));
    logger.Log(Abstraction.Layer.Infrastructure().Action().Faulted(nameof(Main), ex));

     */

    [AttributeUsage(AttributeTargets.Interface)]
    public class AbstractionPropertyAttribute : Attribute
    {
        private readonly string _name;

        public AbstractionPropertyAttribute(string name) => _name = name;

        public override string ToString() => _name;
    }

    public abstract class Abstraction
    {
        /// <summary>
        /// Provides the starting point for all semantic extensions.
        /// </summary>
        public static ILogEntryBuilder<Abstraction>? Layer => default;
    }

    #region Layers

    [AbstractionProperty("Layer")]
    public interface ILogEntryLayer : ILogEntryBuilder<ILogEntryLayer> { }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class AbstractionLayers
    {
        public static IDictionary<string, Option<LogLevel>> Levels { get; set; } = new Dictionary<string, Option<LogLevel>>(SoftString.Comparer)
        {
            [nameof(Business)] = LogLevel.Information,
            [nameof(Service)] = LogLevel.Debug,
            [nameof(Presentation)] = LogLevel.Trace,
            [nameof(IO)] = LogLevel.Trace,
            [nameof(Database)] = LogLevel.Trace,
            [nameof(Network)] = LogLevel.Trace,
        };

        public static ILogEntryBuilder<ILogEntryLayer> Business(this ILogEntryBuilder<Abstraction> builder) => builder.CreateLayerWithCallerName();

        public static ILogEntryBuilder<ILogEntryLayer> Service(this ILogEntryBuilder<Abstraction> builder) => builder.CreateLayerWithCallerName();

        public static ILogEntryBuilder<ILogEntryLayer> Presentation(this ILogEntryBuilder<Abstraction> builder) => builder.CreateLayerWithCallerName();

        public static ILogEntryBuilder<ILogEntryLayer> IO(this ILogEntryBuilder<Abstraction> builder) => builder.CreateLayerWithCallerName();

        public static ILogEntryBuilder<ILogEntryLayer> Database(this ILogEntryBuilder<Abstraction> builder) => builder.CreateLayerWithCallerName();

        public static ILogEntryBuilder<ILogEntryLayer> Network(this ILogEntryBuilder<Abstraction> builder) => builder.CreateLayerWithCallerName();
    }

    public static class AbstractionLayerBuilder
    {
        public static ILogEntryBuilder<ILogEntryLayer> CreateLayerWithCallerName(this ILogEntryBuilder<Abstraction> builder, [CallerMemberName] string? name = null)
        {
            var abstractionProperty = typeof(ILogEntryLayer).GetCustomAttribute<AbstractionPropertyAttribute>().ToString();
            var abstractionLevel = AbstractionLayers.Levels[name!];
            return new LogEntryBuilder<ILogEntryLayer>(LogEntry.Empty(), nameof(Abstraction)).Update(l => l.Add<Log>(abstractionProperty, name).Level(abstractionLevel));
        }
    }

    #endregion

    #region Categories

    [AbstractionProperty("Category")]
    public interface ILogEntryCategory : ILogEntryBuilder<ILogEntryCategory> { }

    public static class AbstractionCategories
    {
        #region Snapshots

        /// <summary>
        /// Logs variables. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Variable(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string? identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        /// <summary>
        /// Logs properties. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Property(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string? identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        /// <summary>
        /// Logs arguments. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Argument(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string? identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        /// <summary>
        /// Logs metadata. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Meta(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string? identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        /// <summary>
        /// Logs performance counters. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Counter(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string? identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        /// <summary>
        /// Logs information about the 'thing' the service is primarily build to work with.
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Subject(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string? identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        public static LogEntry Snapshot(this LogEntry logEntry, object snapshot, string? identifier = default)
        {
            return identifier switch
            {
                {} =>
                logEntry
                    .Add<Log>(LogEntry.Names.SnapshotName, identifier)
                    .Add<Serialize>(LogEntry.Names.Snapshot, snapshot),
                _ =>
                logEntry
                    .Add<Explode>(LogEntry.Names.Snapshot, snapshot)
            };
        }

        #endregion

        /// <summary>
        /// Initializes Routine category.
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Routine(this ILogEntryBuilder<ILogEntryLayer> layer, string identifier)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Add<Log>(LogEntry.Names.SnapshotName, identifier));
        }


        public static ILogEntryBuilder<ILogEntryCategory> Flow(this ILogEntryBuilder<ILogEntryLayer> layer)
        {
            return layer.CreateCategoryWithCallerName();
        }
    }

    public static class AbstractionCategoryBuilder
    {
        /// <summary>
        /// Logs variables. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> CreateCategoryWithCallerName(this ILogEntryBuilder<ILogEntryLayer> layer, [CallerMemberName] string? name = null)
        {
            var abstractionProperty = typeof(ILogEntryCategory).GetCustomAttribute<AbstractionPropertyAttribute>().ToString();
            return new LogEntryBuilder<ILogEntryCategory>(layer).Update(l => l.Add<Log>(abstractionProperty, name));
        }
    }

    #endregion

    #region Routine category helpers

    public static class AbstractionCategoryExtensions
    {
        /// <summary>
        /// Indicates that a routine is still running.
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Running(this ILogEntryBuilder<ILogEntryCategory> category)
        {
            return category.Update(l => l.Add<Log>(LogEntry.Names.Snapshot, nameof(Running)));
        }

        /// <summary>
        /// Indicates that a routine completed its task. Use this only when no errors occured.
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Completed(this ILogEntryBuilder<ILogEntryCategory> category)
        {
            return category.Update(l => l.Add<Log>(LogEntry.Names.Snapshot, nameof(Completed)));
        }

        /// <summary>
        /// Indicates that a routine finished its task. Use this regardless of errors.
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Finished(this ILogEntryBuilder<ILogEntryCategory> category)
        {
            return category.Update(l => l.Add<Log>(LogEntry.Names.Snapshot, nameof(Finished)));
        }

        /// <summary>
        /// Indicates that a routine was canceled and could not complete its task. 
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Canceled(this ILogEntryBuilder<ILogEntryCategory> category)
        {
            return category.Update(l => l.Add<Log>(LogEntry.Names.Snapshot, nameof(Canceled))).Warning();
        }

        /// <summary>
        /// Indicates that a routine faulted before it could complete its task. 
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Faulted(this ILogEntryBuilder<ILogEntryCategory> category, Exception? exception = default)
        {
            return category.Update(l =>
            {
                l.Add<Log>(LogEntry.Names.Snapshot, nameof(Running));
                if (!(exception is null))
                {
                    l.Exception(exception);
                }
            }).Error();
        }

        public static ILogEntryBuilder<ILogEntryCategory> Decision(this ILogEntryBuilder<ILogEntryCategory> category, string decision)
        {
            return category.Update(l => l
                .Add<Log>(LogEntry.Names.SnapshotName, nameof(Decision))
                .Add<Log>(LogEntry.Names.Snapshot, decision)
            );
        }

        /// <summary>
        /// Sets a message that explains why something happened like Canceled a Routine or a Decision.
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Because(this ILogEntryBuilder<ILogEntryCategory> category, string reason)
        {
            return category.Update(l => l.Message(reason));
        }
    }

    #endregion

    public static class AbstractionContextExtensions
    {
        public static ILogEntryBuilder<T> Trace<T>(this ILogEntryBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Trace));

        public static ILogEntryBuilder<T> Debug<T>(this ILogEntryBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Debug));

        public static ILogEntryBuilder<T> Warning<T>(this ILogEntryBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Warning));

        public static ILogEntryBuilder<T> Information<T>(this ILogEntryBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Information));

        public static ILogEntryBuilder<T> Error<T>(this ILogEntryBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Error));

        public static ILogEntryBuilder<T> Fatal<T>(this ILogEntryBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Fatal));
    }
}