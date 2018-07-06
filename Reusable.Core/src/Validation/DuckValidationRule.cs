using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public interface IDuckValidationRule<T>
    {
        DuckValidationRuleOptions Options { get; }
        
        DuckValidationRuleResult<T> Evaluate([CanBeNull] T obj);
    }

    internal class DuckValidationRule<T> : IDuckValidationRule<T>
    {
        private readonly Lazy<string> _expressionString;
        private readonly Lazy<Func<T, bool>> _predicate;
        private readonly Func<T, string> _createMessage;

        public DuckValidationRule([NotNull] Expression<Func<T, bool>> expression, [NotNull] Func<T, string> createMessage, DuckValidationRuleOptions options)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            _predicate = Lazy.Create(expression.Compile);
            _expressionString = Lazy.Create(DuckValidatorRuleExpressionPrettifier.Prettify(expression).ToString);
            _createMessage = createMessage ?? throw new ArgumentNullException(nameof(createMessage));
            Options = options;
        }

        public DuckValidationRuleOptions Options { get; }

        public DuckValidationRuleResult<T> Evaluate(T obj) => new DuckValidationRuleResult<T>(ToString(), _predicate.Value(obj), _createMessage(obj));

        public override string ToString() => _expressionString.Value;

        public static implicit operator string(DuckValidationRule<T> rule) => rule?.ToString();
    }
}