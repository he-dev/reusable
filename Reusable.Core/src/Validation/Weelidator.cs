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
    internal class Weelidator<T> : IWeelidator<T>
    {
        private readonly IList<IWeelidationRule<T>> _rules;

        public Weelidator([NotNull] Action<WeelidatorBuilder<T>> builder)
        {
            var rules = new WeelidatorBuilder<T>();
            builder(rules);
            _rules = rules.Build();            
        }

        public WeelidationResult<T> Validate(T value) => new WeelidationResult<T>(value, Evaluate(value));

        private IEnumerable<WeelidationRuleResult<T>> Evaluate(T value)
        {
            foreach (var (result, options) in _rules.Select(rule => (result: rule.Evaluate(value), options: rule.Options)))
            {
                yield return result;
                if (!result && options.HasFlag(WeelidationRuleOptions.BreakOnFailure))
                {
                    yield break;
                }
            }
        }

        #region IEnumerable

        public IEnumerator<IWeelidationRule<T>> GetEnumerator() => _rules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    public static class Weelidator
    {
        public static IWeelidator<T> For<T>([NotNull] Action<WeelidatorBuilder<T>> builder) => new Weelidator<T>(builder);
    }
}