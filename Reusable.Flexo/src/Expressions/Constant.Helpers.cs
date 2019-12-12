using System.Collections.Generic;
using System.Linq.Custom;

namespace Reusable.Flexo
{
    public static class ConstantHelpers
    {
        public static T Value<T>(this IConstant constant)
        {
            return constant.AsEnumerable<T>().SingleOrThrow
            (
                onEmpty: ("ConstantEmpty", $"Constant '{constant.Id}' does not have a value."),
                onMany: ("ConstantNotScalar", $"Constant '{constant.Id}' has more than one value.")
            );
        }

        public static T ValueOrDefault<T>(this IConstant constant, T defaultValue = default)
        {
            return constant.HasValue ? constant.Value<T>() : defaultValue;
        }

        public static IEnumerable<T> Values<T>(this IConstant constant)
        {
            return constant.AsEnumerable<T>();
        }
    }
}