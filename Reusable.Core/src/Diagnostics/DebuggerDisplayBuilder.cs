using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Diagnostics
{
    public class DebuggerDisplayBuilder<T>
    {
        private readonly IList<(string MemberName, Func<T, object> GetValue)> _members;

        public DebuggerDisplayBuilder()
        {
            _members = new List<(string MemberName, Func<T, object> GetValue)>();
        }

        public DebuggerDisplayBuilder<T> Property<TProperty>(
            [NotNull] Expression<Func<T, TProperty>> propertySelector, 
            [NotNull] string format)
        {
            if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));
            if (format == null) throw new ArgumentNullException(nameof(format));

            var memberExpressions = DebuggerDisplayVisitor.EnumerateMembers(propertySelector);
            var getProperty = propertySelector.Compile();

            return Add(
                memberName: memberExpressions.FormatMemberName(),
                getValue: obj => obj == null ? null : getProperty(obj).FormatValue(format)
            );
        }

        public DebuggerDisplayBuilder<T> Collection<TProperty, TValue>(
            [NotNull] Expression<Func<T, IEnumerable<TProperty>>> propertySelector,
            [NotNull] Expression<Func<TProperty, TValue>> valueSelector,
            [NotNull] string format,
            int max)
        {
            if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));
            if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));
            if (format == null) throw new ArgumentNullException(nameof(format));

            var memberExpressions = DebuggerDisplayVisitor.EnumerateMembers(propertySelector);
            var getProperty = propertySelector.Compile();
            var getValue = valueSelector.Compile();

            return Add(
                memberName: memberExpressions.FormatMemberName(),
                getValue: obj => obj == null ? null : getProperty(obj).Select(getValue).FormatCollection(format, max)
            );
        }

        private DebuggerDisplayBuilder<T> Add(string memberName, Func<T, object> getValue)
        {
            _members.Add((memberName, getValue));
            return this;
        }

        public Func<T, string> Build()
        {
            return obj => string.Join(", ", _members.Select(t => $"{t.MemberName} = {t.GetValue(obj)}"));
        }
    }

    public static class DebuggerDisplayBuilder
    {
        public static DebuggerDisplayBuilder<T> Property<T, TProperty>(
            this DebuggerDisplayBuilder<T> builder,
            Expression<Func<T, TProperty>> propertySelector)
        {
            return builder.Property(propertySelector, DebuggerDisplayFormatter.DefaultValueFormat);
        }

        public static DebuggerDisplayBuilder<T> Collection<T, TProperty, TValue>(
            this DebuggerDisplayBuilder<T> builder,
            Expression<Func<T, IEnumerable<TProperty>>> propertySelector,
            Expression<Func<TProperty, TValue>> valueSelector,
            string format,
            int max = DebuggerDisplayFormatter.DefaultCollectionLength)
        {
            return builder.Collection(propertySelector, valueSelector, DebuggerDisplayFormatter.DefaultValueFormat, max);
        }

        public static DebuggerDisplayBuilder<T> Collection<T, TProperty>(
            this DebuggerDisplayBuilder<T> builder,
            Expression<Func<T, IEnumerable<TProperty>>> propertySelector)
        {
            return builder.Collection(propertySelector, x => x, DebuggerDisplayFormatter.DefaultValueFormat, DebuggerDisplayFormatter.DefaultCollectionLength);
        }
    }
}
