using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.OmniLog.SemanticExtensions
{
    using static AbstractionContext;

    #region Abstractions

    /*
     
     Abstraction
        > Layer (Business | Infrastructure | ...) 
            > Category (Data | Action) 
                > Dump (Object | Property | ...)
     
    logger.Log(Abstraction.Layer.Business().Data().Object(new { customer }));
    logger.Log(Abstraction.Layer.Infrastructure().Data().Variable(new { customer }));
    logger.Log(Abstraction.Layer.Infrastructure().Action().Faulted(nameof(Main), ex));

     */

    // Base interface for the first tier "layer"
    public interface IAbstraction
    {
        IImmutableDictionary<string, object> Values { get; }
    }

    public interface IAbstractionLayer : IAbstraction
    { }

    public interface IAbstractionCategory : IAbstraction
    { }

    #endregion

    #region Implementations

    public abstract class Abstraction
    {
        public static IAbstraction Layer => default;
    }

    #endregion

    #region Layers

    public static class AbstractionExtensions
    {
        public static IAbstractionLayer Business(this IAbstraction abstraction) => new AbstractionContext(PropertyNames.Layer);

        public static IAbstractionLayer Infrastructure(this IAbstraction abstraction) => new AbstractionContext(PropertyNames.Layer);

        public static IAbstractionLayer Presentation(this IAbstraction abstraction) => new AbstractionContext(PropertyNames.Layer);

        public static IAbstractionLayer IO(this IAbstraction abstraction) => new AbstractionContext(PropertyNames.Layer);

        public static IAbstractionLayer Database(this IAbstraction abstraction) => new AbstractionContext(PropertyNames.Layer);

        public static IAbstractionLayer Network(this IAbstraction abstraction) => new AbstractionContext(PropertyNames.Layer);
    }

    #endregion

    #region Categories

    public static class AbstractionLayerExtensions
    {
        /// <summary>
        /// Logs variables. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Variable(this IAbstractionLayer layer, object snapshot) => new AbstractionContext(layer.Values.Add(PropertyNames.Snapshot, snapshot), PropertyNames.Category);

        /// <summary>
        /// Logs properties. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Property(this IAbstractionLayer layer, object snapshot) => new AbstractionContext(layer.Values.Add(PropertyNames.Snapshot, snapshot), PropertyNames.Category);

        /// <summary>
        /// Logs arguments. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Argument(this IAbstractionLayer layer, object snapshot) => new AbstractionContext(layer.Values.Add(PropertyNames.Snapshot, snapshot), PropertyNames.Category);

        /// <summary>
        /// Logs metadata. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Meta(this IAbstractionLayer layer, object snapshot) => new AbstractionContext(layer.Values.Add(PropertyNames.Snapshot, snapshot), PropertyNames.Category);

        /// <summary>
        /// Logs performance counters. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Composite(this IAbstractionLayer layer, object snapshot) => new AbstractionContext(layer.Values.Add(PropertyNames.Snapshot, snapshot), PropertyNames.Category);

        /// <summary>
        /// Logs performance counters. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Counter(this IAbstractionLayer layer, object snapshot) => new AbstractionContext(layer.Values.Add(PropertyNames.Snapshot, snapshot), PropertyNames.Category);

        /// <summary>
        /// Initializes Routine category.
        /// </summary>
        public static IAbstractionCategory Routine(this IAbstractionLayer layer, string identifier) => new AbstractionContext(layer.Values.Add(PropertyNames.Identifier, identifier), PropertyNames.Category);

        public static IAbstractionCategory RoutineFromScope(this IAbstractionLayer layer)
        {
            if (LogScope.Current is null)
            {
                return layer.Routine($"#'{nameof(RoutineFromScope)}' used outside of a scope.");
            }

            // Try to find routine-identifier in the scope hierarchy.
            var scope =
                LogScope
                    .Current
                    .Flatten()
                    .FirstOrDefault(s => s.ContainsKey(nameof(LogScopeExtensions.WithRoutine)));
            return 
                scope is null 
                    ? layer.Routine("#Scope does not contain routine identifier.") 
                    : layer.Routine((string)scope[nameof(LogScopeExtensions.WithRoutine)]);
        }
    }

    #endregion

    #region Routine category helpers

    public static class AbstractionCategoryExtensions
    {
        public static IAbstractionContext Running(this IAbstractionCategory category) => new AbstractionContext(category.Values, PropertyNames.Snapshot);

        public static IAbstractionContext Completed(this IAbstractionCategory category) => new AbstractionContext(category.Values, PropertyNames.Snapshot);

        public static IAbstractionContext Canceled(this IAbstractionCategory category) => new AbstractionContext(category.Values, PropertyNames.Snapshot).Warning();

        public static IAbstractionContext Faulted(this IAbstractionCategory category) => new AbstractionContext(category.Values, PropertyNames.Snapshot).Error();
    }

    #endregion

    public static class AbstractionContextExtensions
    {
        public static IAbstractionContext Trace(this IAbstractionContext context) => new AbstractionContext(context.Values.Add(PropertyNames.LogLevel, LogLevel.Trace));

        public static IAbstractionContext Debug(this IAbstractionContext context) => new AbstractionContext(context.Values.Add(PropertyNames.LogLevel, LogLevel.Debug));

        public static IAbstractionContext Warning(this IAbstractionContext context) => new AbstractionContext(context.Values.Add(PropertyNames.LogLevel, LogLevel.Warning));

        public static IAbstractionContext Information(this IAbstractionContext context) => new AbstractionContext(context.Values.Add(PropertyNames.LogLevel, LogLevel.Information));

        public static IAbstractionContext Error(this IAbstractionContext context) => new AbstractionContext(context.Values.Add(PropertyNames.LogLevel, LogLevel.Error));

        public static IAbstractionContext Fatal(this IAbstractionContext context) => new AbstractionContext(context.Values.Add(PropertyNames.LogLevel, LogLevel.Fatal));
    }
}