using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public interface IDataFuseRule<T>
    {
        DataFuseOptions Options { get; }

        [NotNull]
        IDataFuseResult<T> Evaluate([CanBeNull] T obj);
    }

    internal class DataFuseRule<T> : IDataFuseRule<T>
    {
        private readonly Lazy<Func<T, bool>> _predicate;
        private readonly Func<T, string> _message;
        private readonly Lazy<string> _expressionString;

        public DataFuseRule([NotNull] Expression<Func<T, bool>> expression, [NotNull] Func<T, string> messageFactory, DataFuseOptions options)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            _predicate = Lazy.Create(expression.Compile);
            _expressionString = Lazy.Create(DataFuseExpressionPrettifier.Prettify(expression).ToString);
            _message = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            Options = options;
        }

        public DataFuseOptions Options { get; }

        public IDataFuseResult<T> Evaluate(T obj)
        {
            return new DataFuseResult<T>(_predicate.Value(obj), _expressionString.Value, _message(obj));
        }

        public override string ToString() => _expressionString.Value;

        public static implicit operator string(DataFuseRule<T> rule) => rule?.ToString();
    }
}