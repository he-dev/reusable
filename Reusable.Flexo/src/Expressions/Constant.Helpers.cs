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
            return TryGetValue<T>(constant, out var value) switch
            {
                true => value,
                false => throw DynamicException.Create("ValueType", $"Constant '{constant.Id}' should be of type '{typeof(T).ToPrettyString()}' but is '{constant.Value?.GetType().ToPrettyString()}'.")
            };
        }

        public static T ValueOrDefault<T>(this IConstant constant, T defaultValue = default)
        {
            return TryGetValue<T>(constant, out var value) switch
            {
                true => value,
                false => defaultValue
            };
        }

        private static bool TryGetValue<T>(this IConstant constant, out T value)
        {
            if (typeof(T) == typeof(object))
            {
                value = (T)constant.Value;
                return true;
            }

            if (constant is Constant<T> generic)
            {
                value = generic.Value;
                return true;
            }

            if (constant.Value is T casted)
            {
                value = casted;
                return true;
            }

            value = default;
            return false;
        }
    }
}