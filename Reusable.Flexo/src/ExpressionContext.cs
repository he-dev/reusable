using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Flexo.Diagnostics;

namespace Reusable.Flexo
{
    public interface IExpressionContext
    {
        [NotNull]
        IDictionary<object, object> Items { get; }        

        [NotNull]
        ExpressionMetadata Metadata { get; }
    }

    public class ExpressionContext : IExpressionContext
    {
        public static IExpressionContext Empty => new ExpressionContext();
        
        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();        

        public ExpressionMetadata Metadata { get; } = new ExpressionMetadata();
    }

    public class ExpressionMetadata
    {
        public string DebugView => ExpressionContextScope.Current.ToDebugView();
    }
}