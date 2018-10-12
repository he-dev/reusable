using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public class WeelidatorBuilder<T>
    {
        private readonly IList<WeelidatorRuleBuilder<T>> _ruleBuilders = new List<WeelidatorRuleBuilder<T>>();
        
        public WeelidatorRuleBuilder<T> NewRule([NotNull] Expression<Func<T, bool>> expression)
        {
            var newRule = new WeelidatorRuleBuilder<T>(expression);
            _ruleBuilders.Add(newRule);
            return newRule;
        }

        [NotNull, ItemNotNull]
        public IList<IWeelidationRule<T>> Build() => _ruleBuilders.Select(rb => rb.Build()).ToList();
    }

    public class WeelidatorRuleBuilder<T>
    {
        private readonly Expression<Func<T, bool>> _expression;
        private Func<T, string> _createMessage = _ => string.Empty;
        private WeelidationRuleOptions _options;

        public WeelidatorRuleBuilder([NotNull] Expression<Func<T, bool>> expression)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public WeelidatorRuleBuilder<T> WithMessage(string message)
        {
            _createMessage = _ => message;
            return this;
        }
        
        public WeelidatorRuleBuilder<T> WithMessage(Func<T, string> createMessage)
        {
            _createMessage = createMessage;
            return this;
        }
        
        public WeelidatorRuleBuilder<T> BreakOnFailure()
        {
            _options |= WeelidationRuleOptions.BreakOnFailure;
            return this;
        }
        
        public IWeelidationRule<T> Build()
        {
            return new WeelidationRule<T>(_expression, _createMessage, _options);
        }
    }
}