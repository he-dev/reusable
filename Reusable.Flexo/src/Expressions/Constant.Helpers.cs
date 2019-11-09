using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    public static class ConstantHelpers
    {
        public static IEnumerable<T> Values<T>(this IEnumerable<IConstant> constants)
        {
            return
                from constant in constants
                select constant.Value<T>();
        }

        /// <summary>
        /// Gets the value of a Constant expression if it's of the specified type T or throws an exception.
        /// </summary>
        public static T Value<T>(this IConstant constant)
        {
            if (typeof(T) == typeof(object))
            {
                return (T)constant.Value;
            }
            else
            {
                return
                    constant.Value is T value
                        ? value
                        : throw DynamicException.Create
                        (
                            "ValueType",
                            $"Constant '{constant.Id.ToString()}' should be of type '{typeof(T).ToPrettyString()}' but is '{constant.Value?.GetType().ToPrettyString()}'."
                        );
            }
        }

        public static T ValueOrDefault<T>(this IConstant constant, T defaultValue = default)
        {
            return
                constant.Value is T value
                    ? value
                    : defaultValue;
        }
    }
}