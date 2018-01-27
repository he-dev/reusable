using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.SmartConfig.Utilities.Reflection;

namespace Reusable.SmartConfig.Utilities
{
    internal static class MemberSetter
    {
        public static void Set([NotNull] this LambdaExpression lambdaExpression, [CanBeNull] object value)
        {
            if (lambdaExpression == null) { throw new ArgumentNullException(nameof(lambdaExpression)); }

            var memberExpression = lambdaExpression.MemberExpression();
            var obj = ObjectFinder.FindObject(lambdaExpression);

            switch (memberExpression.Member)
            {
                case PropertyInfo property:
                    if (property.CanWrite)
                    {
                        property.SetValue(obj, value);
                    }
                    // This is a readonly property. We try to write directly to the backing-field.
                    else
                    {
                        var bindingFlags = BindingFlags.NonPublic | (obj == null ? BindingFlags.Static : BindingFlags.Instance);
                        var backingField = (obj?.GetType() ?? property.DeclaringType).GetField($"<{property.Name}>k__BackingField", bindingFlags);
                        if (backingField == null)
                        {
                            throw ("BackingFieldNotFoundException", $"Property {property.Name.QuoteWith("'")} does not have a default backing field.").ToDynamicException();
                        }
                        backingField.SetValue(obj, value);
                    }
                    break;

                case FieldInfo field:
                    field.SetValue(obj, value);
                    break;

                default:
                    throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
            }
        }
    }
}