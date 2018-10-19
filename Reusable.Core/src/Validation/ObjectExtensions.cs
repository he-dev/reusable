using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public static class ObjectExtensions
    {
        [NotNull]
        public static BouncerAudit<T> ValidateWith<T>([NotNull] this T value, [NotNull] IBouncer<T> bouncer) => bouncer.Validate(value);
    }
}