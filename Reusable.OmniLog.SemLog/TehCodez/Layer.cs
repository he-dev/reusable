

using System.Diagnostics;
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
    logger.Log(Abstraction.Layer.Infrastructure().Action().Failed(nameof(Main), ex));

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
    public interface IAbstractionLayerData : IAbstractionLayerCategory
    {
    }

    // Allows to write extensions against the Action catagory.
    public interface IAbstractionLayerAction : IAbstractionLayerCategory
    {
    }

    // The result of building the abstraction.
    public interface IAbstractionContext
    {
        LogLevel LogLevel { get; }

        string LayerName { get; }

        string CategoryName { get; }

        object Dump { get; }
    }

    #endregion

    #region Implementations

    public abstract class Abstraction
    {
        public static IAbstraction Layer => default;
    }

    public class AbstractionLayer : IAbstractionLayer
    {
        private readonly string _name;

        private readonly LogLevel _logLevel;

        public AbstractionLayer(string name, LogLevel logLevel)
        {
            _name = name;
            _logLevel = logLevel;
        }

        public void Deconstruct(out string name, out LogLevel logLevel)
        {
            name = _name;
            logLevel = _logLevel;
        }
    }

    public abstract class AbstractionLayerCategory : IAbstractionLayerCategory
    {
        protected AbstractionLayerCategory(IAbstractionLayer layer, string name)
        {
            Layer = layer;
            Name = name;
        }

        public IAbstractionLayer Layer { get; }

        public string Name { get; }
    }

    public class AbstractionLayerData : AbstractionLayerCategory, IAbstractionLayerData
    {
        public AbstractionLayerData(IAbstractionLayer layer, string name) : base(layer, name) { }
    }

    public class AbstractionLayerAction : AbstractionLayerCategory, IAbstractionLayerAction
    {
        public AbstractionLayerAction(IAbstractionLayer layer, string name) : base(layer, name) { }
    }

    public class AbstractionContext : IAbstractionContext
    {
        public AbstractionContext(IAbstractionLayerCategory layerCategory, object dump)
        {
            (LogLevel, LayerName) = layerCategory.Layer;
            CategoryName = layerCategory.Name;
            Dump = dump;
        }

        public LogLevel LogLevel { get; }

        public string LayerName { get; }

        public string CategoryName { get; }

        public object Dump { get; }
    }

    #endregion

    #region Extensions


    public static class AbstractionExtensions
    {
        #region Information

        public static IAbstractionLayer Business(this IAbstraction abstraction, LogLevel logLevel = null)
        {
            return new AbstractionLayer(nameof(Business), logLevel ?? LogLevel.Information);
        }

        #endregion

        #region Debug

        public static IAbstractionLayer Infrastructure(this IAbstraction abstraction, LogLevel logLevel = null)
        {
            return new AbstractionLayer(nameof(Infrastructure), logLevel ?? LogLevel.Debug);
        }

        #endregion

        #region Trace

        public static IAbstractionLayer Presentation(this IAbstraction abstraction, LogLevel logLevel = null)
        {
            return new AbstractionLayer(nameof(Presentation), logLevel ?? LogLevel.Trace);
        }

        public static IAbstractionLayer IO(this IAbstraction abstraction, LogLevel logLevel = null)
        {
            return new AbstractionLayer(nameof(IO), logLevel ?? LogLevel.Trace);
        }

        public static IAbstractionLayer Database(this IAbstraction abstraction, LogLevel logLevel = null)
        {
            return new AbstractionLayer(nameof(Database), logLevel ?? LogLevel.Trace);
        }

        public static IAbstractionLayer Network(this IAbstraction abstraction, LogLevel logLevel = null)
        {
            return new AbstractionLayer(nameof(Network), logLevel ?? LogLevel.Trace);
        }

        #endregion

        //public static IAbstractionLayer External(this IAbstraction abstraction, LogLevel logLevel = null)
        //{
        //    return new AbstractionLayer(nameof(External), logLevel ?? LogLevel.Trace);
        //}
    }

    public static class AbstractionLayerExtensions
    {
        public static IAbstractionLayerData Data(this IAbstractionLayer layer)
        {
            return new AbstractionLayerData(layer, nameof(Data));
        }

        public static IAbstractionLayerAction Action(this IAbstractionLayer layer)
        {
            return new AbstractionLayerAction(layer, nameof(Action));
        }
    }

    public static class AbstractionLayerDataExtensions
    {
        public static IAbstractionContext Object(this IAbstractionLayerData data, object obj)
        {
            return new AbstractionContext(data, obj);
        }
    }

    public static class AbstractionLayerActionExtensions
    {
        public static IAbstractionContext Started(this IAbstractionLayerAction action, string methodName)
        {
            return new AbstractionContext(action, new { Started = methodName });
        }

        public static IAbstractionContext Finished(this IAbstractionLayerAction action, string methodName)
        {
            return new AbstractionContext(action, new { Finished = methodName });
        }

        public static IAbstractionContext Canceled(this IAbstractionLayerAction action, string methodName)
        {
            return new AbstractionContext(action, new { Canceled = methodName });
        }

        public static IAbstractionContext Failed(this IAbstractionLayerAction action, string methodName)
        {
            return new AbstractionContext(action, new { Failed = methodName });
        }
    }

    #endregion    
}