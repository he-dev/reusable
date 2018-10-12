using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public static class ObjectExtensions
    {
        [NotNull]
        public static WeelidationResult<T> ValidateWith<T>([NotNull] this T value, [NotNull] IWeelidator<T> validator) => validator.Validate(value);
    }
}