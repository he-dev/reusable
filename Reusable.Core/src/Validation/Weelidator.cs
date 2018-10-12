using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public interface IWeelidator<T> : IEnumerable<IWeelidationRule<T>>
    {
        [NotNull]
        WeelidationResult<T> Validate([CanBeNull] T value);
    }

    [PublicAPI]
    public class Weelidator<T> : IWeelidator<T>
    {
        private readonly IList<IWeelidationRule<T>> _rules;

        public Weelidator([NotNull] Action<WeelidatorBuilder<T>> builder)
        {
            var rules = new WeelidatorBuilder<T>();
            builder(rules);
            _rules = rules.Build();
        }

        //public static Weelidator<T> Empty => new Weelidator<T>(_ => { });

        //public DuckValidator<T> Add([NotNull] IDuckValidationRule<T> rule)
        //{
        //    return new DuckValidator<T>(_rules.Append(rule ?? throw new ArgumentNullException(nameof(rule))));
        //}

        public WeelidationResult<T> Validate(T value)
        {           
            var results = new List<IWeelidationRuleResult<T>>();

            foreach (var rule in _rules)
            {
                var result = rule.Evaluate(value);
                results.Add(result);
                if (!result.Success && rule.Options.HasFlag(WeelidationRuleOptions.BreakOnFailure))
                {
                    break;
                }
            }

            return new WeelidationResult<T>(value, results);
        }

        public IEnumerator<IWeelidationRule<T>> GetEnumerator() => _rules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        //public static DuckValidator<T> operator +(DuckValidator<T> duckValidator, IDuckValidationRule<T> rule) => duckValidator.Add(rule);
    }

    public static class Weelidator
    {
        public static IWeelidator<T> For<T>([NotNull] Action<WeelidatorBuilder<T>> builder) => new Weelidator<T>(builder);
    }
}
