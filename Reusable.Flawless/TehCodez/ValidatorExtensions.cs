using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.Flawless
{
    public static class ValidatorExtensions
    {
        [NotNull]
        public static ValidationContext<T> ValidateWith<T>([NotNull] this T obj, [NotNull] IValidator<T> validator)
        {
            return new ValidationContext<T>(obj, validator.Validate(obj).ToList());
        }
    }
}