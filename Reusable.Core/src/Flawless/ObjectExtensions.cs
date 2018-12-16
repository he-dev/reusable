using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public static class ObjectExtensions
    {
        [NotNull]
        public static ExpressValidationResultLookup<T> ValidateWith<T>([NotNull] this T value, [NotNull] IExpressValidator<T> vaccine) => vaccine.Validate(value);
    }
}