using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public interface IValidator<T> : IEnumerable<IValidationRule<T>>
    {
        [NotNull]
        IEnumerable<IValidationResult<T>> Validate([CanBeNull] T obj);
    }

    [PublicAPI]
    public class Validator<T> : IValidator<T>
    {
        private readonly List<IValidationRule<T>> _rules;

        internal Validator([NotNull] IEnumerable<IValidationRule<T>> rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            _rules = rules.ToList();
        }

        public static Validator<T> Empty => new Validator<T>(Enumerable.Empty<IValidationRule<T>>());

        public Validator<T> Add([NotNull] IValidationRule<T> rule)
        {
            return new Validator<T>(_rules.Append(rule ?? throw new ArgumentNullException(nameof(rule))));
        }

        public IEnumerable<IValidationResult<T>> Validate(T obj)
        {
            foreach (var rule in _rules)
            {
                var result = rule.Evaluate(obj);
                yield return result;
                if (!result.Success && rule.Options.HasFlag(ValidationOptions.StopOnFailure))
                {
                    yield break;
                }
            }
        }

        public IEnumerator<IValidationRule<T>> GetEnumerator()
        {
            return _rules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Validator<T> operator +(Validator<T> validator, IValidationRule<T> rule)
        {
            return validator.Add(rule);
        }
    }

    public static class Validator
    {
        public static IValidator<T> Create<T>() => Validator<T>.Empty;
    }
}
