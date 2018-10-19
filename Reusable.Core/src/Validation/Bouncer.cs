using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public interface IBouncer<T> : IEnumerable<IBouncerPolicy<T>>
    {
        [NotNull]
        BouncerAudit<T> Validate([CanBeNull] T value);
    }

    [PublicAPI]
    internal class Bouncer<T> : IBouncer<T>
    {
        private readonly IList<IBouncerPolicy<T>> _rules;

        public Bouncer([NotNull] Action<BouncerBuilder<T>> builder)
        {
            var rules = new BouncerBuilder<T>();
            builder(rules);
            _rules = rules.Build();            
        }

        public BouncerAudit<T> Validate(T value) => new BouncerAudit<T>(value, Evaluate(value));

        private IEnumerable<BouncerPolicyAudit<T>> Evaluate(T value)
        {
            foreach (var (result, options) in _rules.Select(rule => (result: rule.Evaluate(value), options: rule.Options)))
            {
                yield return result;
                if (!result && options.HasFlag(BouncerPolicyOptions.BreakOnFailure))
                {
                    yield break;
                }
            }
        }

        #region IEnumerable

        public IEnumerator<IBouncerPolicy<T>> GetEnumerator() => _rules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    public static class Bouncer
    {
        public static IBouncer<T> For<T>([NotNull] Action<BouncerBuilder<T>> builder) => new Bouncer<T>(builder);
    }
}