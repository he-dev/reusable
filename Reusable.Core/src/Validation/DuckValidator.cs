using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public interface IDuckValidator<T> : IEnumerable<IDuckValidationRule<T>>
    {
        [NotNull]
        DuckValidationResult<T> Validate([CanBeNull] T value);
    }

    [PublicAPI]
    public class DuckValidator<T> : IDuckValidator<T>
    {
        private readonly List<IDuckValidationRule<T>> _rules;

        public DuckValidator([NotNull] Action<DuckValidatorBuilder<T>> builder)
        {
            var rules = new DuckValidatorBuilder<T>();
            builder(rules);
            _rules = rules.ToList();
        }

        public static DuckValidator<T> Empty => new DuckValidator<T>(_ => { });

        //public DuckValidator<T> Add([NotNull] IDuckValidationRule<T> rule)
        //{
        //    return new DuckValidator<T>(_rules.Append(rule ?? throw new ArgumentNullException(nameof(rule))));
        //}

        public DuckValidationResult<T> Validate(T value)
        {           
            var results = new List<IDuckValidationRuleResult<T>>();

            foreach (var rule in _rules)
            {
                var result = rule.Evaluate(value);
                results.Add(result);
                if (!result.Success && rule.Options.HasFlag(DuckValidationRuleOptions.BreakOnFailure))
                {
                    break;
                }
            }

            return new DuckValidationResult<T>(value, results);
        }

        public IEnumerator<IDuckValidationRule<T>> GetEnumerator() => _rules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        //public static DuckValidator<T> operator +(DuckValidator<T> duckValidator, IDuckValidationRule<T> rule) => duckValidator.Add(rule);
    }

    public class DuckValidatorBuilder<T> : List<IDuckValidationRule<T>>
    {
        public new DuckValidatorBuilder<T> Add(IDuckValidationRule<T> rule)
        {
            base.Add(rule);
            return this;
        }
    }

    public static class DataFuse
    {
        public static IDuckValidator<T> For<T>() => DuckValidator<T>.Empty;
    }
}
