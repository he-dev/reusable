using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Validation
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