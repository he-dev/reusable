using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public interface IValidationRule<T>
    {
        ValidationOptions Options { get; }

        [NotNull]
        IValidationResult<T> Evaluate([CanBeNull] T obj);
    }

    internal class ValidationRule<T> : IValidationRule<T>
    {
        private readonly Lazy<Func<T, bool>> _predicate;
        private readonly Func<T, string> _message;
        private readonly Lazy<string> _expressionString;

        public ValidationRule([NotNull] Expression<Func<T, bool>> expression, [NotNull] Func<T, string> messageFactory, ValidationOptions options)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            _predicate = Lazy.Create(expression.Compile);
            _expressionString = Lazy.Create(ValidationExpressionPrettifier.Prettify(expression).ToString);
            _message = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            Options = options;
        }

        public ValidationOptions Options { get; }

        public IValidationResult<T> Evaluate(T obj)
        {
            return new ValidationResult<T>(_predicate.Value(obj), _expressionString.Value, _message(obj));
        }

        public override string ToString() => _expressionString.Value;

        public static implicit operator string(ValidationRule<T> rule) => rule?.ToString();
    }
}