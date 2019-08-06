using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.OmniLog.Middleware;
using Reusable.OmniLog.SemanticExtensions.Middleware;

namespace Reusable.OmniLog.SemanticExtensions
{
    #region Layers

    public static class AbstractionLayers
    {
        public static IAbstractionBuilder<IAbstractionLayer> Business(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();

        public static IAbstractionBuilder<IAbstractionLayer> Service(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();

        public static IAbstractionBuilder<IAbstractionLayer> Presentation(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();

        // ReSharper disable once InconsistentNaming
        public static IAbstractionBuilder<IAbstractionLayer> IO(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();

        public static IAbstractionBuilder<IAbstractionLayer> Database(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();

        public static IAbstractionBuilder<IAbstractionLayer> Network(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();
    }

    #endregion

    #region Categories

    public static class AbstractionCategoryBuilder
    {
        /// <summary>
        /// Logs variables. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> CreateCategoryWithCallerName(this IAbstractionBuilder<IAbstractionLayer> layer, [CallerMemberName] string name = null)
        {
            var abstractionProperty = typeof(IAbstractionCategory).GetCustomAttribute<AbstractionPropertyAttribute>().ToString();
            return new AbstractionBuilder<IAbstractionCategory>(layer.Build()).Update(l => l.SetItem(abstractionProperty, default, name));
        }
    }

    public static class AbstractionCategories
    {
        /// <summary>
        /// Logs variables. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Variable(this IAbstractionBuilder<IAbstractionLayer> layer, object snapshot)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Variable), LoggerDump.LogItemTag, snapshot));
        }

        /// <summary>
        /// Logs properties. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Property(this IAbstractionBuilder<IAbstractionLayer> layer, object snapshot)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Property), LoggerDump.LogItemTag, snapshot));
        }

        /// <summary>
        /// Logs arguments. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Argument(this IAbstractionBuilder<IAbstractionLayer> layer, object snapshot)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Argument), LoggerDump.LogItemTag, snapshot));
        }

        /// <summary>
        /// Logs metadata. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Meta(this IAbstractionBuilder<IAbstractionLayer> layer, object snapshot)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Meta), LoggerDump.LogItemTag, snapshot));
        }

        /// <summary>
        /// Logs performance counters. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Counter(this IAbstractionBuilder<IAbstractionLayer> layer, object snapshot)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Counter), LoggerDump.LogItemTag, snapshot));
        }

        /// <summary>
        /// Initializes Routine category.
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Routine(this IAbstractionBuilder<IAbstractionLayer> layer, string identifier)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Routine), LoggerDump.LogItemTag, identifier));
        }

        public static IAbstractionBuilder<IAbstractionCategory> Decision(this IAbstractionBuilder<IAbstractionLayer> layer, string description)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Decision), LoggerDump.LogItemTag, description));
        }

        //        public static IAbstractionCategory RoutineFromScope(this IAbstractionLayer layer)
        //        {
        //            if (LogScope.Current is null)
        //            {
        //                return layer.Routine($"#'{nameof(RoutineFromScope)}' used outside of a scope.");
        //            }
        //
        //            // Try to find routine-identifier in the scope hierarchy.
        //            var scope =
        //                LogScope
        //                    .Current
        //                    .Flatten()
        //                    .FirstOrDefault(s => s.ContainsKey(nameof(Routine)));
        //            return
        //                scope is null
        //                    ? layer.Routine("#Scope does not contain routine identifier.")
        //                    : layer.Routine((string)scope[nameof(Routine)]);
        //        }
    }

    #endregion

    #region Routine category helpers

    public static class AbstractionCategoryExtensions
    {
        private static readonly string Category = nameof(AbstractionCategories.Routine);
        private static readonly string Tag = LoggerDump.LogItemTag;

        public static IAbstractionBuilder<IAbstractionCategory> Running(this IAbstractionBuilder<IAbstractionCategory> category)
        {
            return category.Update(l => l.SetItem(Category, Tag, new Dictionary<object, object> { [l[Category, Tag]] = nameof(Running) }));
        }

        public static IAbstractionBuilder<IAbstractionCategory> Completed(this IAbstractionBuilder<IAbstractionCategory> category)
        {
            return category.Update(l => l.SetItem(Category, Tag, new Dictionary<object, object> { [l[Category, Tag]] = nameof(Completed) }));
        }

        public static IAbstractionBuilder<IAbstractionCategory> Canceled(this IAbstractionBuilder<IAbstractionCategory> category)
        {
            return category.Update(l => l.SetItem(Category, Tag, new Dictionary<object, object> { [l[Category, Tag]] = nameof(Canceled) })).Warning();
        }

        public static IAbstractionBuilder<IAbstractionCategory> Faulted(this IAbstractionBuilder<IAbstractionCategory> category)
        {
            return category.Update(l => l.SetItem(Category, Tag, new Dictionary<object, object> { [l[Category, Tag]] = nameof(Faulted) })).Error();
        }

        /// <summary>
        /// Sets a message that explains why something happened like Canceled a Routine or a Decision.
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Because(this IAbstractionBuilder<IAbstractionCategory> category, string reason)
        {
            return category.Update(l => l.Message(reason));
        }
    }

    #endregion

    public static class AbstractionContextExtensions
    {
        public static IAbstractionBuilder<T> Trace<T>(this IAbstractionBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Trace));

        public static IAbstractionBuilder<T> Debug<T>(this IAbstractionBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Debug));

        public static IAbstractionBuilder<T> Warning<T>(this IAbstractionBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Warning));

        public static IAbstractionBuilder<T> Information<T>(this IAbstractionBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Information));

        public static IAbstractionBuilder<T> Error<T>(this IAbstractionBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Error));

        public static IAbstractionBuilder<T> Fatal<T>(this IAbstractionBuilder<T> builder) => builder.Update(l => l.Level(LogLevel.Fatal));
    }
}