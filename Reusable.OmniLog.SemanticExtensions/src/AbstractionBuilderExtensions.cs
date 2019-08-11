using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;

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
        public static ILogEntryBuilder<Abstraction> Layer => default;
    }

    #region Layers

    [AbstractionProperty("Layer")]
    public interface ILogEntryLayer : ILogEntryBuilder<ILogEntryLayer> { }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class AbstractionLayers
    {
        public static IDictionary<string, LogLevel> Levels { get; set; } = new Dictionary<string, LogLevel>(SoftString.Comparer)
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
        public static ILogEntryBuilder<ILogEntryLayer> CreateLayerWithCallerName(this ILogEntryBuilder<Abstraction> builder, [CallerMemberName] string name = null)
        {
            var abstractionProperty = typeof(ILogEntryLayer).GetCustomAttribute<AbstractionPropertyAttribute>().ToString();
            var abstractionLevel = AbstractionLayers.Levels[name];
            return new LogEntryBuilder<ILogEntryLayer>(LogEntry.Empty(), nameof(Abstraction)).Update(l => l.SetItem(abstractionProperty, default, name).Level(abstractionLevel));
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
        public static ILogEntryBuilder<ILogEntryCategory> Variable(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        /// <summary>
        /// Logs properties. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Property(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        /// <summary>
        /// Logs arguments. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Argument(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        /// <summary>
        /// Logs metadata. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Meta(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        /// <summary>
        /// Logs performance counters. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Counter(this ILogEntryBuilder<ILogEntryLayer> layer, object snapshot, string identifier = default)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.Snapshot(snapshot, identifier));
        }

        private static LogEntry Snapshot(this LogEntry logEntry, object snapshot, string identifier = default)
        {
            if (identifier is null)
            {
                return
                    logEntry
                        .SetItem(LogEntry.Names.Snapshot, LogEntry.Tags.Explodable, snapshot);
            }
            else
            {
                return
                    logEntry
                        .SetItem(LogEntry.Names.Object, LogEntry.Tags.Loggable, identifier)
                        .SetItem(LogEntry.Names.Snapshot, LogEntry.Tags.Serializable, snapshot);
            }
        }

        #endregion

        /// <summary>
        /// Initializes Routine category.
        /// </summary>
        public static ILogEntryBuilder<ILogEntryCategory> Routine(this ILogEntryBuilder<ILogEntryLayer> layer, string identifier)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(LogEntry.Names.Object, LogEntry.Tags.Loggable, identifier));
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
        public static ILogEntryBuilder<ILogEntryCategory> CreateCategoryWithCallerName(this ILogEntryBuilder<ILogEntryLayer> layer, [CallerMemberName] string name = null)
        {
            var abstractionProperty = typeof(ILogEntryCategory).GetCustomAttribute<AbstractionPropertyAttribute>().ToString();
            return new LogEntryBuilder<ILogEntryCategory>(layer).Update(l => l.SetItem(abstractionProperty, default, name));
        }
    }

    #endregion

    #region Routine category helpers

    public static class AbstractionCategoryExtensions
    {
        public static ILogEntryBuilder<ILogEntryCategory> Running(this ILogEntryBuilder<ILogEntryCategory> category)
        {
            return category.Update(l => l.SetItem(LogEntry.Names.Snapshot, LogEntry.Tags.Loggable, nameof(Running)));
        }

        public static ILogEntryBuilder<ILogEntryCategory> Completed(this ILogEntryBuilder<ILogEntryCategory> category)
        {
            return category.Update(l => l.SetItem(LogEntry.Names.Snapshot, LogEntry.Tags.Loggable, nameof(Completed)));
        }

        public static ILogEntryBuilder<ILogEntryCategory> Canceled(this ILogEntryBuilder<ILogEntryCategory> category)
        {
            return category.Update(l => l.SetItem(LogEntry.Names.Snapshot, LogEntry.Tags.Loggable, nameof(Canceled))).Warning();
        }

        public static ILogEntryBuilder<ILogEntryCategory> Faulted(this ILogEntryBuilder<ILogEntryCategory> category, Exception exception = default)
        {
            return category.Update(l =>
            {
                l.SetItem(LogEntry.Names.Snapshot, LogEntry.Tags.Loggable, nameof(Running));
                if (!(exception is null))
                {
                    l.Exception(exception);
                }
            }).Error();
        }

        public static ILogEntryBuilder<ILogEntryCategory> Decision(this ILogEntryBuilder<ILogEntryCategory> category, string decision)
        {
            return category.Update(l => l
                .SetItem(LogEntry.Names.Object, LogEntry.Tags.Loggable, nameof(Decision))
                .SetItem(LogEntry.Names.Snapshot, LogEntry.Tags.Loggable, decision)
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