

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.OmniLog.SemanticExtensions
{
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
    public interface IAbstraction { }

    public interface IAbstractionLayer
    {
        void Deconstruct(out string name, out LogLevel logLevel);
    }

    // Hides the two properties from the extensions.
    public interface IAbstractionLayerCategory
    {
        IAbstractionLayer Layer { get; }

        string Name { get; }
    }

    // Allows to write extensions against the Data catagory.
    //public interface IAbstractionLayerData : IAbstractionLayerCategory { }

    // Allows to write extensions against the Routine catagory.
    public interface IAbstractionLayerRoutine : IAbstractionLayerCategory
    {
        string Identifier { get; }
    }

    // The result of building the abstraction.
    

    #endregion

    #region Implementations

    public abstract class Abstraction
    {
        public static IAbstraction Layer => default;
    }

    public class AbstractionLayer : IAbstractionLayer
    {
        public static IDictionary<SoftString, LogLevel> LogLevels = new Dictionary<SoftString, LogLevel>
        {
            [nameof(AbstractionExtensions.Business)] = LogLevel.Information,
            [nameof(AbstractionExtensions.Infrastructure)] = LogLevel.Debug,
            [nameof(AbstractionExtensions.Core)] = LogLevel.Debug,
            [nameof(AbstractionExtensions.Presentation)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.IO)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.Database)] = LogLevel.Trace,
            [nameof(AbstractionExtensions.Network)] = LogLevel.Trace,
        };

        private readonly string _name;

        public AbstractionLayer([CallerMemberName] string name = null)
        {
            _name = name;
        }

        public void Deconstruct(out string name, out LogLevel logLevel)
        {
            name = _name;
            logLevel = LogLevels[_name];
        }
    }

    public class AbstractionLayerCategory : IAbstractionLayerCategory
    {
        public AbstractionLayerCategory(IAbstractionLayer layer, [CallerMemberName] string name = null)
        {
            Layer = layer;
            Name = name;
        }

        public IAbstractionLayer Layer { get; }

        public string Name { get; }
    }

    public class AbstractionLayerRoutine : AbstractionLayerCategory, IAbstractionLayerRoutine
    {
        public AbstractionLayerRoutine(IAbstractionLayer layer, string identifier, [CallerMemberName] string categoryName = null)
            : base(layer, categoryName)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }
    }

    #endregion

    #region Layers

    public static class AbstractionExtensions
    {
        public static IAbstractionLayer Business(this IAbstraction abstraction) => new AbstractionLayer();
        public static IAbstractionLayer Infrastructure(this IAbstraction abstraction) => new AbstractionLayer();
        public static IAbstractionLayer Core(this IAbstraction abstraction) => new AbstractionLayer();
        public static IAbstractionLayer Presentation(this IAbstraction abstraction) => new AbstractionLayer();
        public static IAbstractionLayer IO(this IAbstraction abstraction) => new AbstractionLayer();
        public static IAbstractionLayer Database(this IAbstraction abstraction) => new AbstractionLayer();
        public static IAbstractionLayer Network(this IAbstraction abstraction) => new AbstractionLayer();
    }

    #endregion

    #region Categories

    public static class AbstractionLayerExtensions
    {
        /// <summary>
        /// Logs variables. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Variable(this IAbstractionLayer layer, object dump) => new AbstractionContext(layer, dump);

        /// <summary>
        /// Logs properties. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Property(this IAbstractionLayer layer, object dump) => new AbstractionContext(layer, dump);

        /// <summary>
        /// Logs arguments. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Argument(this IAbstractionLayer layer, object dump) => new AbstractionContext(layer, dump);

        /// <summary>
        /// Logs metadata. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Meta(this IAbstractionLayer layer, object dump) => new AbstractionContext(layer, dump);

        /// <summary>
        /// Logs performance counters. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Composite(this IAbstractionLayer layer, object dump) => new AbstractionContext(layer, dump);

        /// <summary>
        /// Logs performance counters. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Counter(this IAbstractionLayer layer, object dump) => new AbstractionContext(layer, dump);

        public static IAbstractionLayerRoutine Routine(this IAbstractionLayer layer, string identifier) => new AbstractionLayerRoutine(layer, identifier);
    }

    #endregion

    #region Routine category helpers

    public static class AbstractionLayerRoutineExtensions
    {
        public static IAbstractionContext Running(this IAbstractionLayerRoutine category)
        {
            return new AbstractionContext(category.Layer, new Dictionary<string, object> {[category.Identifier] = nameof(Running)}, category.Name);
        }

        public static IAbstractionContext Completed(this IAbstractionLayerRoutine category)
        {
            return new AbstractionContext(category.Layer, new Dictionary<string, object> {[category.Identifier] = nameof(Completed)}, category.Name);
        }

        public static IAbstractionContext Canceled(this IAbstractionLayerRoutine category)
        {
            return new AbstractionContext(category.Layer, new Dictionary<string, object> {[category.Identifier] = nameof(Canceled)}, category.Name);
        }

        public static IAbstractionContext Faulted(this IAbstractionLayerRoutine category)
        {
            return new AbstractionContext(category.Layer, new Dictionary<string, object> {[category.Identifier] = nameof(Faulted)}, category.Name);
        }
    }

    #endregion
}