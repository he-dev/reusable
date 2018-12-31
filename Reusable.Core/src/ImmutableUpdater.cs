using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable
{
    public static class ImmutableUpdater
    {
        [NotNull]
        public static T With<T, TMember>(this T obj, Expression<Func<T, TMember>> memberSelector, TMember newValue)
        {
            if (!(memberSelector.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException($"You must select a member. Affected expression '{memberSelector}'.");
            }

            if (!(memberExpression.Member is PropertyInfo selectedProperty))
            {
                throw new ArgumentException($"You must select a property. Affected expression '{memberSelector}'.");
            }

            if (selectedProperty.GetSetMethod() != null)
            {
                throw new ArgumentException($"You must select a readonly property. Affected expression '{memberSelector}'.");
            }

            if (GetBackingField<T>(selectedProperty.Name) == null)
            {
                throw new ArgumentException($"You must select a pure readonly property (not a computed one). Affected expression '{memberSelector}'.");
            }

            var updates =
                from property in obj.ImmutableProperties()
                let getsUpdated = property.Name == selectedProperty.Name
                select
                (
                    property,
                    getsUpdated ? newValue : property.GetValue(obj)
                );

            return (T)ImmutableUpdateConstructor(typeof(T)).Invoke(new object[] { new ImmutableUpdate(updates) });
        }

        public static void Bind<T>(this ImmutableUpdate update, T obj)
        {
            // todo - fix invalid stack-frame when optimized
            // var isCalledByImmutableUpdateCtor = new StackFrame(1).GetMethod() == ImmutableUpdateConstructor(typeof(T));
            // if (!isCalledByImmutableUpdateCtor)
            // {
            //     throw new InvalidOperationException($"You can call '{nameof(Bind)}' only from within an ImmutableUpdate constructor.");
            // }

            foreach (var (property, value) in update)
            {
                GetBackingField<T>(property.Name)?.SetValue(obj, value);
            }
        }

        private static FieldInfo GetBackingField<T>(string propertyName)
        {
            var backingFieldBindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            var backingFieldName = $"<{propertyName}>k__BackingField";
            return typeof(T).GetField(backingFieldName, backingFieldBindingFlags);
        }

        private static IEnumerable<PropertyInfo> ImmutableProperties<T>(this T obj)
        {
            return
                typeof(T)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.IsReadonly());
        }

        private static bool IsReadonly(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetSetMethod() is null;
        }

        private static ConstructorInfo ImmutableUpdateConstructor(Type type)
        {
            return type.GetConstructor(new[] { typeof(ImmutableUpdate) });
        }
    }

    public sealed class ImmutableUpdate : IEnumerable<(PropertyInfo Property, object Value)>
    {
        private readonly IEnumerable<(PropertyInfo Property, object Value)> _updates;

        internal ImmutableUpdate(IEnumerable<(PropertyInfo Property, object Value)> updates)
        {
            _updates = updates;
        }

        public IEnumerator<(PropertyInfo Property, object Value)> GetEnumerator() => _updates.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _updates.GetEnumerator();
    }   
}