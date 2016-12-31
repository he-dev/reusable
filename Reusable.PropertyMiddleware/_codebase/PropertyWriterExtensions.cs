using System;
using System.Linq.Expressions;

namespace Reusable.PropertyMiddleware
{
    public static class PropertyWriterExtensions
    {
        public static PropertyWriter<T> SetValue<T, TValue>(this PropertyWriter<T> writer, T obj, Expression<Func<T, TValue>> property, TValue value)
        {
            var propertyName = property.GetMemberName();
            return writer.SetValue(obj, propertyName, value);
        }

        public static PropertyWriter<T> SetValues<T>(this PropertyWriter<T> writer, T obj, ExpressionDictionary<T> properties)
        {
            foreach (var property in properties)
            {
                writer.SetValue(obj, property.Key, property.Value);
            }
            return writer;
        }
    }
}