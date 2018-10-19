using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Reflection;

namespace Reusable.Validation
{
    public class BouncerBuilder<T>
    {
        private readonly IList<BouncerPolicyBuilder<T>> _ruleBuilders = new List<BouncerPolicyBuilder<T>>();
        
        [NotNull]
        public BouncerPolicyBuilder<T> NewRule([NotNull] Expression<Func<T, bool>> expression)
        {
            var newRule = new BouncerPolicyBuilder<T>(expression);
            _ruleBuilders.Add(newRule);
            return newRule;
        }

        [NotNull, ItemNotNull]
        internal IList<IBouncerPolicy<T>> Build()
        {
            if (_ruleBuilders.Empty()) throw new InvalidOperationException("You need to define at least one validation rule.");
            return _ruleBuilders.Select(rb => rb.Build()).ToList();
        }
    }

    public class BouncerPolicyBuilder<T>
    {
        private readonly Expression<Func<T, bool>> _expression;
        private Func<T, string> _createMessage = _ => string.Empty;
        private BouncerPolicyOptions _options;

        public BouncerPolicyBuilder([NotNull] Expression<Func<T, bool>> expression)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        [NotNull]
        public BouncerPolicyBuilder<T> WithMessage(string message)
        {
            _createMessage = _ => message;
            return this;
        }
        
        [NotNull]
        public BouncerPolicyBuilder<T> WithMessage(Func<T, string> createMessage)
        {
            _createMessage = createMessage;
            return this;
        }
        
        [NotNull]
        public BouncerPolicyBuilder<T> BreakOnFailure()
        {
            _options |= BouncerPolicyOptions.BreakOnFailure;
            return this;
        }
        
        [NotNull]
        public IBouncerPolicy<T> Build()
        {
            return new BouncerPolicy<T>(_expression, _createMessage, _options);
        }
    }
}