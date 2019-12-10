using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    public static class ConstantHelpers
    {
        public static T ValueOrDefault<T>(this IConstant constant, T defaultValue = default)
        {
            foreach (var value in constant)
            {
                return value switch
                {
                    T t => t,
                    _ => defaultValue
                };
            }

            throw DynamicException.Create("ValueType", $"Constant '{constant.Id}' should be of type '{typeof(T).ToPrettyString()}' but is '{constant.ValueType.ToPrettyString()}'.");
        }
    }
}