using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Services
{
    public static class MemberExpressionExtensions
    {
        public static void Apply<T>([NotNull] this Expression<Func<T>> expression, [CanBeNull] object value)
        {
            if (expression == null) { throw new ArgumentNullException(nameof(expression)); }
            if (expression.Body is MemberExpression memberExpression)
            {
                var obj = GetObject(memberExpression.Expression);

                switch (memberExpression.Member.MemberType)
                {
                    case MemberTypes.Property:
                        var property = (PropertyInfo)memberExpression.Member;
                        if (property.CanWrite)
                        {
                            ((PropertyInfo)memberExpression.Member).SetValue(obj, value);
                        }
                        else
                        {
                            var bindingFlags = BindingFlags.NonPublic | (obj == null ? BindingFlags.Static : BindingFlags.Instance);
                            var backingField = (obj?.GetType() ?? property.DeclaringType).GetField($"<{property.Name}>k__BackingField", bindingFlags);
                            if (backingField == null)
                            {
                                throw new BackingFieldNotFoundException(property.Name);
                            }
                            backingField.SetValue(obj, value);
                        }
                        break;
                    case MemberTypes.Field:
                        ((FieldInfo)memberExpression.Member).SetValue(obj, value);
                        break;
                    default:
                        throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
                }
            }
            else
            {
                throw new ArgumentException($"Expression must be a {nameof(MemberExpression)}.");
            }
        }

        public static object Select<T>([NotNull] this Expression<Func<T>> expression)
        {
            if (expression == null) { throw new ArgumentNullException(nameof(expression)); }
            if (expression.Body is MemberExpression memberExpression)
            {
                var obj = GetObject(memberExpression.Expression);

                switch (memberExpression.Member.MemberType)
                {
                    case MemberTypes.Property:
                        var property = (PropertyInfo)memberExpression.Member;
                        return property.GetValue(obj);                        
                    case MemberTypes.Field:
                        return ((FieldInfo)memberExpression.Member).GetValue(obj);
                    default:
                        throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
                }
            }
            else
            {
                throw new ArgumentException($"Expression must be a {nameof(MemberExpression)}.");
            }
        }

        private static object GetObject(Expression expression)
        {
            // This is a static class.
            if (expression == null)
            {
                return null;
            }
            if (expression is MemberExpression anonymousMemberExpression)
            {
                // Extract constant value from the anonyous-wrapper
                var container = ((ConstantExpression)anonymousMemberExpression.Expression).Value;
                return ((FieldInfo)anonymousMemberExpression.Member).GetValue(container);
            }
            else
            {
                return ((ConstantExpression)expression).Value;
            }
        }
    }
}