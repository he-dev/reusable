using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public static class ObjectExtensions
    {
        [NotNull]
        public static DuckValidationResult<T> ValidateWith<T>([NotNull] this T value, [NotNull] IDuckValidator<T> validator) => validator.Validate(value);
    }
}