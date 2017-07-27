using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Services
{
    internal static class MemberSetter
    {
        public static void Apply([NotNull] this LambdaExpression lambdaExpression, [CanBeNull] object value)
        {
            if (lambdaExpression == null) { throw new ArgumentNullException(nameof(lambdaExpression)); }
            if (lambdaExpression.Body is MemberExpression memberExpression)
            {
                var obj = memberExpression.GetObject();

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
    }
}