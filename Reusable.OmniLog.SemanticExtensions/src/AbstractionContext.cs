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
        LogEntry Build();
    }

    // Base interface for the first tier "layer"
    public interface IAbstractionBuilder<T> : IAbstractionBuilder
    {
        IAbstractionBuilder<T> Update(AlterLogEntryCallback alterLogEntry);
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

    public abstract class Abstraction
    {
        /// <summary>
        /// Provides the starting point for all semantic extensions.
        /// </summary>
        public static IAbstractionBuilder<object> Layer => default;
    }

    public readonly struct AbstractionBuilder<T> : IAbstractionBuilder<T>
    {
        private readonly LogEntry _logEntry;

        public AbstractionBuilder(LogEntry logEntry) => _logEntry = logEntry;

        public IAbstractionBuilder<T> Update(AlterLogEntryCallback alterLogEntry) => this.Do(self => alterLogEntry(self._logEntry));

        public LogEntry Build() => _logEntry ?? LogEntry.Empty();
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


    
}