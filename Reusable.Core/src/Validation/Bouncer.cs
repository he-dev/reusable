using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Validation
{
    public interface IBouncer<T> : IEnumerable<IBouncerPolicy<T>>
    {
        [NotNull, ItemNotNull]
        BouncerPolicyCheckLookup<T> Validate([CanBeNull] T value);
    }

    [PublicAPI]
    internal class Bouncer<T> : IBouncer<T>
    {
        private readonly IList<IBouncerPolicy<T>> _policies;

        public Bouncer([NotNull] Action<BouncerBuilder<T>> builder)
        {
            var rules = new BouncerBuilder<T>();
            builder(rules);
            _policies = rules.Build();
        }

        public BouncerPolicyCheckLookup<T> Validate(T value) => new BouncerPolicyCheckLookup<T>(value, Evaluate(value).ToLookup(t => t.IsFollowed));

        private IEnumerable<BouncerPolicyCheck<T>> Evaluate(T value)
        {
            foreach (var policy in _policies)
            {
                var policyCheck = policy.Evaluate(value);
                yield return policyCheck;
                if (!policyCheck.IsFollowed && policy.Options.HasFlag(BouncerPolicyOptions.BreakOnFailure))
                {
                    yield break;
                }
            }
        }

        #region IEnumerable

        public IEnumerator<IBouncerPolicy<T>> GetEnumerator() => _policies.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    public static class Bouncer
    {
        public static IBouncer<T> For<T>([NotNull] Action<BouncerBuilder<T>> builder) => new Bouncer<T>(builder);
    }
}