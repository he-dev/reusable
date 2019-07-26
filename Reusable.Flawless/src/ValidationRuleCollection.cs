using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public static class ValidationRuleCollection
    {
        public static ValidationRuleCollection<T, TContext> For<T, TContext>()
        {
            return ValidationRuleCollection<T,TContext>.Empty;
        }

        public static ValidationRuleCollection<T, object> For<T>()
        {
            return For<T, object>();
        }
    }

    public class ValidationRuleCollection<T, TContext> : IEnumerable<IValidationRule<T, TContext>>
    {
        private readonly IImmutableList<IValidationRule<T, TContext>> _rules;

        public ValidationRuleCollection(IImmutableList<IValidationRule<T, TContext>> rules)
        {
            _rules = rules.ToImmutableList();
        }
        
        public static ValidationRuleCollection<T, TContext> Empty { get; } = new ValidationRuleCollection<T, TContext>(ImmutableList<IValidationRule<T, TContext>>.Empty);

        public ValidationRuleCollection<T, TContext> Add(IValidationRule<T, TContext> rule)
        {
            return new ValidationRuleCollection<T, TContext>(_rules.Add(rule));
        }

        public IEnumerator<IValidationRule<T, TContext>> GetEnumerator()
        {
            return _rules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_rules).GetEnumerator();
        }
    }

    public class ValidationRuleCollection<T> : ValidationRuleCollection<T, object>
    {
        public ValidationRuleCollection(IImmutableList<IValidationRule<T, object>> rules) : base(rules) { }
    }
}