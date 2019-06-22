using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public class ValidationRuleBuilder<T> //: IValidationRuleBuilder<T>
    {
        private readonly ValidationRuleOption _option;

        private Expression<Func<T, bool>> _predicate;

        public ValidationRuleBuilder(ValidationRuleOption option)
        {
            _option = option;
            _predicate = _ => true;
        }

        public ValidationRuleBuilder<T> Predicate(Expression<Func<T, bool>> expression)
        {
            _predicate = expression;
            return this;
        }

        public ValidationRuleBuilder<T> Message(Expression<Func<string>> message)
        {
            return this;
        }

        [NotNull]
        public IValidationRule<T, TContext> Build<TContext>()
        {
            return new ValidationRule<T, TContext>(_predicate, default, _option);
        }
    }
}