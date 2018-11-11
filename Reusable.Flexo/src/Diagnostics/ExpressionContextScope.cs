using System;
using System.Diagnostics;
using System.Threading;
using Reusable.Diagnostics;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Expressions;

namespace Reusable.Flexo.Diagnostics
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class ExpressionContextScope : IDisposable
    {
        private readonly IExpression _expression;
        private readonly IExpressionContext _context;

        // ReSharper disable once InconsistentNaming - This cannot be renamed because it'd confilict with the property that has the same name.
        private static readonly AsyncLocal<ExpressionContextScope> _current = new AsyncLocal<ExpressionContextScope>();

        private ExpressionContextScope(IExpression expression, IExpressionContext context, int depth)
        {
            _expression = expression;
            _context = context;
            Metadata.Path = Metadata.Path.Add(expression.Name);
            Depth = depth;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.Property(x => x.Depth);
        });

        public ExpressionContextScope Parent { get; private set; }

        public static ExpressionContextScope Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        public int Depth { get; }

        public IExpression Result { get; set; }

        private ExpressionContextMetadata Metadata => _context.Metadata;

        public static ExpressionContextScope Push(IExpression expression, IExpressionContext context)
        {
            var scope = Current = new ExpressionContextScope(expression, context, Current?.Depth + 1 ?? 0)
            {
                Parent = Current
            };
            return scope;
        }

        public void Dispose()
        {
            Current = Current.Parent;

            const int indentWitdh = 4;
            var indentString = new string(' ', indentWitdh * _context.Metadata.Path.Count);

            var expressionTypeName = _expression.GetType().Name;
            var resultExpressionName = Result.Name;
            var isSelf = expressionTypeName == resultExpressionName;
            var result = Result is IConstant constant ? constant.Value : Result;
            Metadata.Log = Metadata.Log.Insert(0, $"{indentString}{expressionTypeName}{(isSelf ? string.Empty : $"[{resultExpressionName}]")}: \"{result}");// ({string.Join("/", Metadata.Path)})");
            Metadata.Path = Metadata.Path.RemoveAt(Metadata.Path.Count - 1);
        }
    }
}