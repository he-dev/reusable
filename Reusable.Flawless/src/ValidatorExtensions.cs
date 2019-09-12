using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    [PublicAPI]
    public static class ValidatorExtensions
    {
        public static IEnumerable<IValidationResult> Validate<T>(this IValidator<T> validator, T obj)
        {
            return validator.Validate(obj, ImmutableContainer.Empty);
        }
        
        [NotNull]
        public static IEnumerable<IValidationResult> ValidateWith<T>(this T obj, Validator<T> validator, IImmutableContainer context)
        {
            return validator.Validate(obj, context);
        }

        [NotNull]
        public static IEnumerable<IValidationResult> ValidateWith<T>(this T obj, Validator<T> validator)
        {
            return obj.ValidateWith(validator, ImmutableContainer.Empty);
        }
    }
}