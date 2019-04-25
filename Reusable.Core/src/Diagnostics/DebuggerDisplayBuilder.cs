using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Diagnostics
{
    public class DebuggerDisplayBuilder<T>
    {
        private readonly IList<(string MemberName, Func<T, object> GetValue)> _members;

        public DebuggerDisplayBuilder()
        {
            _members = new List<(string MemberName, Func<T, object> GetValue)>();
        }

        public DebuggerDisplayBuilder<T> Property<TProperty>
        (
            [NotNull] Expression<Func<T, TProperty>> memberSelector,
            [NotNull] string format
        )
        {
            if (memberSelector == null) throw new ArgumentNullException(nameof(memberSelector));
            if (format == null) throw new ArgumentNullException(nameof(format));

            var memberExpressions = DebuggerDisplayVisitor.EnumerateMembers(memberSelector);
            var getProperty = memberSelector.Compile();

            return Add
            (
                memberName: memberExpressions.FormatMemberName(),
                getValue: obj => obj == null ? null : getProperty(obj).FormatValue(format)
            );
        }

        public DebuggerDisplayBuilder<T> Collection<TProperty, TValue>
        (
            [NotNull] Expression<Func<T, IEnumerable<TProperty>>> memberSelector,
            [NotNull] Expression<Func<TProperty, TValue>> itemSelector,
            [NotNull] string format,
            int max
        )
        {
            if (memberSelector == null) throw new ArgumentNullException(nameof(memberSelector));
            if (itemSelector == null) throw new ArgumentNullException(nameof(itemSelector));
            if (format == null) throw new ArgumentNullException(nameof(format));

            var memberExpressions = DebuggerDisplayVisitor.EnumerateMembers(memberSelector);
            var getProperty = memberSelector.Compile();
            var getValue = itemSelector.Compile();

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
        public static DebuggerDisplayBuilder<T> DisplayValue<T, TProperty>
        (
            this DebuggerDisplayBuilder<T> builder,
            Expression<Func<T, TProperty>> memberSelector
        )
        {
            return builder.Property(memberSelector, DebuggerDisplayFormatter.DefaultValueFormat);
        }

        public static DebuggerDisplayBuilder<T> DisplayValues<T, TProperty, TValue>
        (
            this DebuggerDisplayBuilder<T> builder,
            Expression<Func<T, IEnumerable<TProperty>>> memberSelector,
            Expression<Func<TProperty, TValue>> itemSelector,
            string format,
            int max = DebuggerDisplayFormatter.DefaultCollectionLength
        )
        {
            return builder.Collection(memberSelector, itemSelector, DebuggerDisplayFormatter.DefaultValueFormat, max);
        }

        public static DebuggerDisplayBuilder<T> DisplayValues<T, TProperty>
        (
            this DebuggerDisplayBuilder<T> builder,
            Expression<Func<T, IEnumerable<TProperty>>> memberSelector
        )
        {
            return builder.Collection(memberSelector, x => x, DebuggerDisplayFormatter.DefaultValueFormat, DebuggerDisplayFormatter.DefaultCollectionLength);
        }
    }
}