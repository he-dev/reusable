using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Reusable.Flexo.Expressions
{
    public interface IExpressionContext
    {
        [NotNull]
        IDictionary<object, object> Items { get; }

        [NotNull]
        IDictionary<string, object> Parameters { get; }

        [NotNull]
        ExpressionContextMetadata Metadata { get; }
    }

    public class ExpressionContext : IExpressionContext
    {
        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();

        public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public ExpressionContextMetadata Metadata { get; } = new ExpressionContextMetadata();
    }

    public class ExpressionContextMetadata
    {
        public IImmutableList<string> Path { get; set; } = ImmutableList.Create<string>();

        public IImmutableList<string> Log { get; set; } = ImmutableList.Create<string>();

        public string DebugView => string.Join(Environment.NewLine, Log);
    }
}