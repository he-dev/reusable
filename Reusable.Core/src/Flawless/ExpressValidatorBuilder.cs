using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public class ExpressValidatorBuilder<T>
    {
        private readonly IList<ExpressValidationRuleBuilder<T>> _policyBuilders = new List<ExpressValidationRuleBuilder<T>>();
        
        [NotNull]
        public ExpressValidationRuleBuilder<T> Policy([NotNull] Expression<Func<T, bool>> expression)
        {
            var policyBuilder = new ExpressValidationRuleBuilder<T>(expression);
            _policyBuilders.Add(policyBuilder);
            return policyBuilder;
        }

        [NotNull, ItemNotNull]
        internal IList<IExpressValidationRule<T>> Build()
        {
            if (_policyBuilders.Empty()) throw new InvalidOperationException("You need to define at least one validation rule.");
            return _policyBuilders.Select(rb => rb.Build()).ToList();
        }
    }

    public class ExpressValidationRuleBuilder<T>
    {
        private readonly Expression<Func<T, bool>> _expression;
        private Func<T, string> _createMessage = _ => "N/A";
        private ExpressValidationOptions _options;

        public ExpressValidationRuleBuilder([NotNull] Expression<Func<T, bool>> expression)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        [NotNull]
        public ExpressValidationRuleBuilder<T> WithMessage(string message)
        {
            _createMessage = _ => message;
            return this;
        }
        
        [NotNull]
        public ExpressValidationRuleBuilder<T> WithMessage(Func<T, string> createMessage)
        {
            _createMessage = createMessage;
            return this;
        }
        
        [NotNull]
        public ExpressValidationRuleBuilder<T> BreakOnFailure()
        {
            _options |= ExpressValidationOptions.BreakOnFailure;
            return this;
        }
        
        [NotNull]
        public IExpressValidationRule<T> Build()
        {
            return new ExpressValidationRule<T>(_expression, _createMessage, _options);
        }
    }
}