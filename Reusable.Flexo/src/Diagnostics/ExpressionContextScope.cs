using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Reusable.Diagnostics;

namespace Reusable.Flexo.Diagnostics
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class ExpressionContextScope : IDisposable
    {
        // ReSharper disable once InconsistentNaming - This cannot be renamed because it'd confilict with the property that has the same name.
        private static readonly AsyncLocal<ExpressionContextScope> _current = new AsyncLocal<ExpressionContextScope>();

        private ExpressionContextScope(IExpression expression, IExpressionContext context, int depth)
        {
            Expression = expression;
            Context = context;
            Depth = depth;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayMember(x => x.Depth);
        });

        public ExpressionContextScope Parent { get; private set; }

        public static ExpressionContextScope Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        public IExpression Expression { get; }

        public IExpressionContext Context { get; }

        public int Depth { get; }

        public static ExpressionContextScope Push(IExpression expression, IExpressionContext context)
        {
            var scope = Current = new ExpressionContextScope(expression, context, Current?.Depth + 1 ?? 0)
            {
                Parent = Current
            };
            return scope;
        }

        public void Dispose() => Current = Current.Parent;
    }

    public static class ExpressionContextScopeExtensions
    {
        private const int IndentWidth = 4;

        public static string ToDebugView(this ExpressionContextScope scope)
        {
            var scopes = new Stack<ExpressionContextScope>(scope.Flatten());

            var debugView = new StringBuilder();

            foreach (var inner in scopes)
            {
                debugView
                    .Append(IndentString(inner.Depth))
                    .Append(inner.Expression.Name)
                    .Append(inner.Expression is IConstant constant ? $": {constant.Value}"  : default)
                    .AppendLine();
            }

            return debugView.ToString();
        }

        private static string IndentString(int depth) => new string(' ', IndentWidth * depth);

        public static IEnumerable<ExpressionContextScope> Flatten(this ExpressionContextScope scope)
        {
            var current = scope;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }
    }
}