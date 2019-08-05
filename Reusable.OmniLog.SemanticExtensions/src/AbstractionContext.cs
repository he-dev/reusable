using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Middleware;

namespace Reusable.OmniLog.SemanticExtensions
{
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

    public interface IAbstractionBuilder
    {
        Log Build();
    }

    // Base interface for the first tier "layer"
    public interface IAbstractionBuilder<T> : IAbstractionBuilder
    {
        IAbstractionBuilder<T> Update(AlterLog alterLog);
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class AbstractionPropertyAttribute : Attribute
    {
        private readonly string _name;

        public AbstractionPropertyAttribute(string name) => _name = name;

        public override string ToString() => _name;
    }

    [AbstractionProperty("Layer")]
    public interface IAbstractionLayer : IAbstractionBuilder<IAbstractionLayer> { }

    [AbstractionProperty("Category")]
    public interface IAbstractionCategory : IAbstractionBuilder<IAbstractionCategory> { }

//    public interface IAbstractionContext : IAbstractionLayer, IAbstractionCategory
//    {
//        Log Log { get; }
//    }

    public abstract class Abstraction
    {
        /// <summary>
        /// Provides the starting point for all semantic extensions.
        /// </summary>
        public static IAbstractionBuilder<object> Layer => default;
    }

    public readonly struct AbstractionBuilder<T> : IAbstractionBuilder<T>
    {
        private readonly Log _log;

        public AbstractionBuilder(Log log) => _log = log;

        public IAbstractionBuilder<T> Update(AlterLog alterLog) => this.Do(self => alterLog(self._log));

        public Log Build() => _log ?? Log.Empty();
    }

    public static class AbstractionLayerBuilder
    {
        public static IAbstractionBuilder<IAbstractionLayer> CreateLayerWithCallerName(this IAbstractionBuilder<object> builder, [CallerMemberName] string name = null)
        {
            var abstractionProperty = typeof(IAbstractionLayer).GetCustomAttribute<AbstractionPropertyAttribute>().ToString();
            return new AbstractionBuilder<IAbstractionLayer>(Log.Empty()).Update(l => l.SetItem((abstractionProperty, default), name));
        }
    }

    #endregion


    public static class ObjectExtensions
    {
        public static IEnumerable<(string Name, object Value)> EnumerateProperties<T>(this T obj)
        {
            return
                obj is IDictionary<string, object> dictionary
                    ? dictionary.Select(item => (item.Key, item.Value))
                    : obj
                        .GetType()
                        //.ValidateIsAnonymous()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(property => (property.Name, property.GetValue(obj)));
        }

        private static Type ValidateIsAnonymous(this Type type)
        {
            var isAnonymous = type.Name.StartsWith("<>f__AnonymousType");

            return
                isAnonymous
                    ? type
                    : throw DynamicException.Create("Snapshot", "Snapshot must be either an anonymous type or a dictionary");
        }
    }
}