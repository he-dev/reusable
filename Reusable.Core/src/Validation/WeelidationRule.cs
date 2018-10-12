using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public interface IWeelidationRule<T>
    {
        WeelidationRuleOptions Options { get; }
        
        WeelidationRuleResult<T> Evaluate([CanBeNull] T obj);
    }

    internal class WeelidationRule<T> : IWeelidationRule<T>
    {
        private readonly Lazy<string> _expressionString;
        private readonly Lazy<Func<T, bool>> _predicate;
        private readonly Func<T, string> _createMessage;

        public WeelidationRule([NotNull] Expression<Func<T, bool>> expression, [NotNull] Func<T, string> createMessage, WeelidationRuleOptions options)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            _predicate = Lazy.Create(expression.Compile);
            _expressionString = Lazy.Create(WeelidatorRuleExpressionPrettifier.Prettify(expression).ToString);
            _createMessage = createMessage ?? throw new ArgumentNullException(nameof(createMessage));
            Options = options;
        }

        public WeelidationRuleOptions Options { get; }

        public WeelidationRuleResult<T> Evaluate(T obj) => new WeelidationRuleResult<T>(ToString(), _predicate.Value(obj), _createMessage(obj));

        public override string ToString() => _expressionString.Value;

        public static implicit operator string(WeelidationRule<T> rule) => rule?.ToString();
    }
}