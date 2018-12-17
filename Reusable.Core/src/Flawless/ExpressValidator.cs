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
        ExpressValidationResultLookup<T> Validate([CanBeNull] T value);
    }

    [PublicAPI]
    internal class ExpressValidator<T> : IExpressValidator<T>
    {
        private readonly IList<IExpressValidationRule<T>> _rules;

        public ExpressValidator([NotNull] Action<ExpressValidationBuilder<T>> builder)
        {
            var rules = new ExpressValidationBuilder<T>();
            builder(rules);
            _rules = rules.Build();
        }

        public ExpressValidationResultLookup<T> Validate(T value) => new ExpressValidationResultLookup<T>(value, Evaluate(value).ToLookup(t => t.Success));

        private IEnumerable<ExpressValidationResult<T>> Evaluate(T value)
        {
            foreach (var rule in _rules)
            {
                var result = rule.Evaluate(value);
                yield return result;
                if (!result.Success && rule.Options.HasFlag(ExpressValidationOptions.BreakOnFailure))
                {
                    yield break;
                }
            }
        }

        #region IEnumerable

        public IEnumerator<IExpressValidationRule<T>> GetEnumerator() => _rules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    public static class ExpressValidator
    {
        public static IExpressValidator<T> For<T>([NotNull] Action<ExpressValidationBuilder<T>> builder) => new ExpressValidator<T>(builder);
    }
}