using System;
using System.Linq.Expressions;

namespace Reusable.Flawless
{
    public class ValidationRule<T>
    {
        private readonly Lazy<string> _expressionString;

        private readonly Lazy<Func<T, bool>> _predicate;

        public ValidationRule(Expression<Func<T, bool>> expression, ValidationOptions options)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            _predicate = new Lazy<Func<T, bool>>(() => expression.Compile());
            _expressionString = new Lazy<string>(() => CreateExpressionString(expression));
            Options = options;
        }

        public ValidationOptions Options { get; }

        private static string CreateExpressionString(Expression<Func<T, bool>> expression)
        {
            var typeParameterReplacement = Expression.Parameter(typeof(T), $"<{typeof(T).Name}>");
            return ValidationExpressionPrettifier.Prettify(expression.Body, expression.Parameters[0], typeParameterReplacement).ToString();
        }

        public bool IsMet(T obj) => _predicate.Value(obj);

        public override string ToString() => _expressionString.Value;

        public static implicit operator string(ValidationRule<T> rule) => rule?.ToString();
    }
}