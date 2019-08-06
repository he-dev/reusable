using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.SemanticExtensions
{
    #region Layers
    
    [AbstractionProperty("Layer")]
    public interface IAbstractionLayer : IAbstractionBuilder<IAbstractionLayer> { }


    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class AbstractionLayers
    {
        public static IAbstractionBuilder<IAbstractionLayer> Business(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();

        public static IAbstractionBuilder<IAbstractionLayer> Service(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();

        public static IAbstractionBuilder<IAbstractionLayer> Presentation(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();

        public static IAbstractionBuilder<IAbstractionLayer> IO(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();

        public static IAbstractionBuilder<IAbstractionLayer> Database(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();

        public static IAbstractionBuilder<IAbstractionLayer> Network(this IAbstractionBuilder<object> builder) => builder.CreateLayerWithCallerName();
    }

    public static class AbstractionLayerBuilder
    {
        public static IAbstractionBuilder<IAbstractionLayer> CreateLayerWithCallerName(this IAbstractionBuilder<object> builder, [CallerMemberName] string name = null)
        {
            var abstractionProperty = typeof(IAbstractionLayer).GetCustomAttribute<AbstractionPropertyAttribute>().ToString();
            return new AbstractionBuilder<IAbstractionLayer>(LogEntry.Empty()).Update(l => l.SetItem(abstractionProperty, default, name));
        }
    }

    #endregion

    #region Categories
    
    [AbstractionProperty("Category")]
    public interface IAbstractionCategory : IAbstractionBuilder<IAbstractionCategory> { }


    public static class AbstractionCategories
    {
        /// <summary>
        /// Logs variables. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Variable(this IAbstractionBuilder<IAbstractionLayer> layer, object snapshot)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Variable), DumpNode.LogItemTag, snapshot));
        }

        /// <summary>
        /// Logs properties. The dump must be an anonymous type with at least one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Property(this IAbstractionBuilder<IAbstractionLayer> layer, object snapshot)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Property), DumpNode.LogItemTag, snapshot));
        }

        /// <summary>
        /// Logs arguments. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Argument(this IAbstractionBuilder<IAbstractionLayer> layer, object snapshot)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Argument), DumpNode.LogItemTag, snapshot));
        }

        /// <summary>
        /// Logs metadata. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Meta(this IAbstractionBuilder<IAbstractionLayer> layer, object snapshot)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Meta), DumpNode.LogItemTag, snapshot));
        }

        /// <summary>
        /// Logs performance counters. The dump must be an anonymous type with at leas one property: new { foo[, bar] }
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Counter(this IAbstractionBuilder<IAbstractionLayer> layer, object snapshot)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Counter), DumpNode.LogItemTag, snapshot));
        }

        /// <summary>
        /// Initializes Routine category.
        /// </summary>
        public static IAbstractionBuilder<IAbstractionCategory> Routine(this IAbstractionBuilder<IAbstractionLayer> layer, string identifier)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Routine), DumpNode.LogItemTag, identifier));
        }

        public static IAbstractionBuilder<IAbstractionCategory> Decision(this IAbstractionBuilder<IAbstractionLayer> layer, string description)
        {
            return layer.CreateCategoryWithCallerName().Update(l => l.SetItem(nameof(Decision), DumpNode.LogItemTag, description));
        }
    }

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

    #endregion

    #region Routine category helpers

    public static class AbstractionCategoryExtensions
    {
        private static readonly string Category = nameof(AbstractionCategories.Routine);
        private static readonly string Tag = DumpNode.LogItemTag;

        public static IAbstractionBuilder<IAbstractionCategory> Running(this IAbstractionBuilder<IAbstractionCategory> category)
        {
            return category.Update(l => l.SetItem(Category, Tag, new Dictionary<string, object> { [l[Category, Tag].ToString()] = nameof(Running) }));
        }

        public static IAbstractionBuilder<IAbstractionCategory> Completed(this IAbstractionBuilder<IAbstractionCategory> category)
        {
            return category.Update(l => l.SetItem(Category, Tag, new Dictionary<string, object> { [l[Category, Tag].ToString()] = nameof(Completed) }));
        }

        public static IAbstractionBuilder<IAbstractionCategory> Canceled(this IAbstractionBuilder<IAbstractionCategory> category)
        {
            return category.Update(l => l.SetItem(Category, Tag, new Dictionary<string, object> { [l[Category, Tag].ToString()] = nameof(Canceled) })).Warning();
        }

        public static IAbstractionBuilder<IAbstractionCategory> Faulted(this IAbstractionBuilder<IAbstractionCategory> category)
        {
            return category.Update(l => l.SetItem(Category, Tag, new Dictionary<string, object> { [l[Category, Tag].ToString()] = nameof(Faulted) })).Error();
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