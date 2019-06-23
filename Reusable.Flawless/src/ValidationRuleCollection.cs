using System.Collections;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public static class ValidationRuleCollection
    {
        public static IImmutableList<IValidationRule<T, TContext>> For<T, TContext>() => ImmutableList<IValidationRule<T, TContext>>.Empty;

        public static IImmutableList<IValidationRule<T, object>> For<T>() => For<T, object>();
    }
}