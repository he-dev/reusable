using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Reusable.ExpressProperty
{
    public static class PropertyReaderExtensions
    {
        public static TValue GetValue<T, TValue>(this PropertyReader<T> reader, T obj, Expression<Func<T, TValue>> property)
        {
            var propertyName = property.GetMemberName();
            return reader.GetValue<TValue>(obj, propertyName);
        }

        public static Dictionary<string, object> GetValues<T>(this PropertyReader<T> reader, T obj, params string[] propertyNames)
        {
            if (obj == null) { throw new ArgumentNullException(paramName: nameof(obj)); }
            if (propertyNames == null) { throw new ArgumentNullException(paramName: nameof(propertyNames)); }

            return propertyNames.ToDictionary(
                propertyName => propertyName,
                propertyName => reader.GetValue<object>(obj, propertyName)
            );
        }

        public static Dictionary<string, object> GetValues<T>(this PropertyReader<T> reader, T obj, ExpressionList<T> properties)
        {
            return properties.Select(p => p.GetMemberName()).ToDictionary(
                propertyName => propertyName,
                propertyName => reader.GetValue<object>(obj, propertyName));
        }
    }
}