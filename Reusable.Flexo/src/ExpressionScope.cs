using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;

namespace Reusable.Flexo
{
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class ExpressionScope : IDisposable
    {
        // ReSharper disable once InconsistentNaming - This cannot be renamed because it'd conflict with the property that has the same name.
        private static readonly AsyncLocal<ExpressionScope> _current = new AsyncLocal<ExpressionScope>();

        static ExpressionScope() { }

        private ExpressionScope(int depth)
        {
            Depth = depth;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            //builder.Property(x => x.CorrelationId);
            //builder.Property(x => x.CorrelationContext);
            builder.DisplayScalar(x => x.Depth);
        });

        public ExpressionScope Parent { get; private set; }

        /// <summary>
        /// Gets the current log-scope which is the deepest one.
        /// </summary>
        [CanBeNull]
        public static ExpressionScope Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        public int Depth { get; }

        public IImmutableContainer Context { get; private set; }

        public static ExpressionScope Push(IImmutableContainer context)
        {
            var scope = Current = new ExpressionScope(Current?.Depth + 1 ?? 0)
            {
                Parent = Current,
                Context = context
            };
            return scope;
        }

        public void Dispose()
        {
            Current = Current?.Parent;
        }
    }
}