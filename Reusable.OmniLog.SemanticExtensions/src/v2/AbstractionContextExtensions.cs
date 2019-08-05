﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.OmniLog.SemanticExtensions.v2
{
    using acpn = AbstractionContext.PropertyNames;
    
    #region Abstractions

    /*
     
     Abstraction
        > Layer (Business | Infrastructure | ...) 
            > Category (Data | Action) 
                > Snapshot (Object | Property | ...)
     
    logger.Log(Abstraction.Layer.Business().Data().Object(new { customer }));
    logger.Log(Abstraction.Layer.Infrastructure().Data().Variable(new { customer }));
    logger.Log(Abstraction.Layer.Infrastructure().Action().Faulted(nameof(Main), ex));

     */

    // Base interface for the first tier "layer"
    public interface IAbstraction
    {
        object GetItem([NotNull] string name);
        
        IAbstractionContext SetItem([NotNull] string name, [NotNull] object value);
    }

    public interface IAbstractionLayer : IAbstraction { }

    public interface IAbstractionCategory : IAbstraction { }

    #endregion

    #region Implementations

    public abstract class Abstraction
    {
        /// <summary>
        /// Provides the starting point for all semantic extensions.
        /// </summary>
        public static IAbstraction Layer => default;
    }

    #endregion

    #region Layers

    public static class AbstractionExtensions
    {
        public static IAbstractionLayer Business(this IAbstraction abstraction) => AbstractionFactory.CreateWithCallerName();

        public static IAbstractionLayer Service(this IAbstraction abstraction) => AbstractionFactory.CreateWithCallerName();

        public static IAbstractionLayer Presentation(this IAbstraction abstraction) => AbstractionFactory.CreateWithCallerName();

        // ReSharper disable once InconsistentNaming
        public static IAbstractionLayer IO(this IAbstraction abstraction) => AbstractionFactory.CreateWithCallerName();

        public static IAbstractionLayer Database(this IAbstraction abstraction) => AbstractionFactory.CreateWithCallerName();

        public static IAbstractionLayer Network(this IAbstraction abstraction) => AbstractionFactory.CreateWithCallerName();
    }

    public static class AbstractionFactory
    {
        public static IAbstractionLayer CreateWithCallerName([CallerMemberName] string name = null)
        {
            // ReSharper disable once AssignNullToNotNullAttribute - The compiler takes care of it.
            return AbstractionContext.Empty.SetItem(acpn.Layer, name);
        }
    }

    #endregion

    #region Categories

    public static class AbstractionLayerExtensions
    {
        /// <summary>
        /// Logs variables. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Variable(this IAbstractionLayer layer, object snapshot)
        {
            return
                layer
                    .SetItem(acpn.Category, nameof(Variable))
                    .SetItem(acpn.Snapshot, snapshot);
        }

        /// <summary>
        /// Logs properties. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Property(this IAbstractionLayer layer, object snapshot)
        {
            return
                layer
                    .SetItem(acpn.Category, nameof(Property))
                    .SetItem(acpn.Snapshot, snapshot);
        }

        /// <summary>
        /// Logs arguments. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Argument(this IAbstractionLayer layer, object snapshot)
        {
            return
                layer
                    .SetItem(acpn.Category, nameof(Argument))
                    .SetItem(acpn.Snapshot, snapshot);
        }

        /// <summary>
        /// Logs metadata. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Meta(this IAbstractionLayer layer, object snapshot)
        {
            return
                layer
                    .SetItem(acpn.Category, nameof(Meta))
                    .SetItem(acpn.Snapshot, snapshot);
        }

        /// <summary>
        /// Logs performance counters. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Composite(this IAbstractionLayer layer, object snapshot)
        {
            return
                layer
                    .SetItem(acpn.Category, nameof(Composite))
                    .SetItem(acpn.Snapshot, snapshot);
        }

        /// <summary>
        /// Logs performance counters. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionContext Counter(this IAbstractionLayer layer, object snapshot)
        {
            return
                layer
                    .SetItem(acpn.Category, nameof(Counter))
                    .SetItem(acpn.Snapshot, snapshot);
        }

        /// <summary>
        /// Initializes Routine category.
        /// </summary>
        public static IAbstractionCategory Routine(this IAbstractionLayer layer, string identifier)
        {
            return
                layer
                    .SetItem(acpn.Category, nameof(Routine))
                    .SetItem(acpn.Routine, identifier);
        }

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
                    .FirstOrDefault(s => s.ContainsKey(nameof(Routine)));
            return
                scope is null
                    ? layer.Routine("#Scope does not contain routine identifier.")
                    : layer.Routine((string)scope[nameof(Routine)]);
        }

        public static IAbstractionContext Decision(this IAbstractionLayer layer, object description)
        {
            return
                layer
                    .SetItem(acpn.Category, "Flow")
                    .SetItem(acpn.Snapshot, new { Decision = description });
        }
    }

    #endregion

    #region Routine category helpers

    public static class AbstractionCategoryExtensions
    {
        public static IAbstractionContext Running(this IAbstractionCategory category)
        {
            var identifier = (string)category.GetItem(acpn.Routine);
            return category.SetItem(acpn.Snapshot, new Dictionary<string, object> { [identifier] = nameof(Running) });
        }

        public static IAbstractionContext Completed(this IAbstractionCategory category)
        {
            var identifier = (string)category.GetItem(acpn.Routine);
            return category.SetItem(acpn.Snapshot, new Dictionary<string, object> { [identifier] = nameof(Completed) });
        }

        public static IAbstractionContext Canceled(this IAbstractionCategory category)
        {
            var identifier = (string)category.GetItem(acpn.Routine);
            return category.SetItem(acpn.Snapshot, new Dictionary<string, object> { [identifier] = nameof(Canceled) }).Warning();
        }

        public static IAbstractionContext Faulted(this IAbstractionCategory category)
        {
            var identifier = (string)category.GetItem(acpn.Routine);
            return category.SetItem(acpn.Snapshot, new Dictionary<string, object> { [identifier] = nameof(Faulted) }).Error();
        }

        /// <summary>
        /// Sets a message that explains why something happened like Canceled a Routine or a Decision.
        /// </summary>
        public static IAbstractionContext Because(this IAbstractionContext category, string reason)
        {
            return category.SetItem(acpn.Because, reason);
        }
    }

    #endregion

    public static class AbstractionContextExtensions
    {
        public static IAbstractionContext Trace(this IAbstractionContext context) => context.SetItem(acpn.Level, LogLevel.Trace);

        public static IAbstractionContext Debug(this IAbstractionContext context) => context.SetItem(acpn.Level, LogLevel.Debug);

        public static IAbstractionContext Warning(this IAbstractionContext context) => context.SetItem(acpn.Level, LogLevel.Warning);

        public static IAbstractionContext Information(this IAbstractionContext context) => context.SetItem(acpn.Level, LogLevel.Information);

        public static IAbstractionContext Error(this IAbstractionContext context) => context.SetItem(acpn.Level, LogLevel.Error);

        public static IAbstractionContext Fatal(this IAbstractionContext context) => context.SetItem(acpn.Level, LogLevel.Fatal);
    }
}