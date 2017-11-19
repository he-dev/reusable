using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    [PublicAPI]
    public class Validator<T> : IEnumerable<ValidationRule<T>>
    {
        private readonly List<ValidationRule<T>> _rules;

        public Validator([NotNull] IEnumerable<ValidationRule<T>> rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            _rules = rules.ToList();
        }

        public static Validator<T> Empty => new Validator<T>(Enumerable.Empty<ValidationRule<T>>());

        public Validator<T> Add([NotNull] ValidationRule<T> rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            return new Validator<T>(_rules.Concat(new[] { rule }));
        }

        public IEnumerable<IValidation<T>> Validate(T obj)
        {
            foreach (var rule in _rules)
            {
                if (rule.IsMet(obj))
                {
                    yield return PassedValidation<T>.Create(rule);
                }
                else
                {
                    yield return FailedValidation<T>.Create(rule);
                    if (rule.Options.HasFlag(ValidationOptions.StopOnFailure))
                    {
                        yield break;
                    }
                }
            }
        }

        public IEnumerator<ValidationRule<T>> GetEnumerator()
        {
            return _rules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Validator<T> operator +(Validator<T> validator, ValidationRule<T> rule)
        {
            return validator.Add(rule);
        }
    }
}
