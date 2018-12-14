using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public interface IExpressValidator<T> : IEnumerable<IExpressValidationRule<T>>
    {
        [NotNull, ItemNotNull]
        ExpressValidationResultLookup<T> IsThreat([CanBeNull] T value);
    }

    [PublicAPI]
    internal class ExpressValidator<T> : IExpressValidator<T>
    {
        private readonly IList<IExpressValidationRule<T>> _agents;

        public ExpressValidator([NotNull] Action<ExpressValidatorBuilder<T>> builder)
        {
            var rules = new ExpressValidatorBuilder<T>();
            builder(rules);
            _agents = rules.Build();
        }

        public ExpressValidationResultLookup<T> IsThreat(T value) => new ExpressValidationResultLookup<T>(value, Evaluate(value).ToLookup(t => t.IsFollowed));

        private IEnumerable<ExpressValidationResult<T>> Evaluate(T value)
        {
            foreach (var agent in _agents)
            {
                var policyCheck = agent.Evaluate(value);
                yield return policyCheck;
                if (!policyCheck.IsFollowed && agent.Options.HasFlag(ExpressValidationOptions.BreakOnFailure))
                {
                    yield break;
                }
            }
        }

        #region IEnumerable

        public IEnumerator<IExpressValidationRule<T>> GetEnumerator() => _agents.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    public static class ExpressValidator
    {
        public static IExpressValidator<T> For<T>([NotNull] Action<ExpressValidatorBuilder<T>> builder) => new ExpressValidator<T>(builder);
    }
}