using System;
using System.Collections.Generic;
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

        public DebuggerDisplayBuilder<T> DisplayScalar<TSource>
        (
            Expression<Func<T, TSource>> sourceSelector,
            string formatString = DebuggerDisplayFormatter.DefaultFormatString
        )
        {
            var memberExpressions = DebuggerDisplayVisitor.EnumerateMembers(sourceSelector);
            var getProperty = sourceSelector.Compile();

            return Add
            (
                memberName: memberExpressions.FormatMemberName(),
                getValue: obj => obj is {} ? getProperty(obj)?.FormatValue(formatString) : default
            );
        }

        public DebuggerDisplayBuilder<T> DisplayEnumerable<TSource, TItem>
        (
            Expression<Func<T, IEnumerable<TSource>>> sourceSelector,
            Expression<Func<TSource, TItem>> itemSelector,
            string formatString = DebuggerDisplayFormatter.DefaultFormatString,
            int max = DebuggerDisplayFormatter.DefaultEnumerableTake
        )
        {
            var memberExpressions = DebuggerDisplayVisitor.EnumerateMembers(sourceSelector);
            var getProperty = sourceSelector.Compile();
            var getValue = itemSelector.Compile();

            return Add
            (
                memberName: memberExpressions.FormatMemberName(),
                getValue: obj => obj is {} ? getProperty(obj)?.Select(getValue).FormatCollection(formatString, max) : default
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
        // public static DebuggerDisplayBuilder<T> DisplayScalar<T, TProperty>
        // (
        //     this DebuggerDisplayBuilder<T> builder,
        //     Expression<Func<T, TProperty>> sourceSelector,
        //     [NotNull] string formatString = DebuggerDisplayFormatter.DefaultFormatString,
        //     int max = DebuggerDisplayFormatter.DefaultEnumerableTake
        // )
        // {
        //     return builder.DisplayScalar<TProperty>(sourceSelector, x => x, formatString, max);
        // }

        // public static DebuggerDisplayBuilder<T> DisplayEnumerable<T, TProperty, TValue>
        // (
        //     this DebuggerDisplayBuilder<T> builder,
        //     Expression<Func<T, IEnumerable<TProperty>>> memberSelector,
        //     Expression<Func<TProperty, TValue>> itemSelector,
        //     string format = DebuggerDisplayFormatter.DefaultFormatString,
        //     int max = DebuggerDisplayFormatter.DefaultEnumerableTake
        // )
        // {
        //     return builder.DisplayEnumerable(memberSelector, itemSelector, format ?? DebuggerDisplayFormatter.DefaultFormatString, max);
        // }

        public static DebuggerDisplayBuilder<T> DisplayEnumerable<T, TProperty>
        (
            this DebuggerDisplayBuilder<T> builder,
            Expression<Func<T, IEnumerable<TProperty>>> sourceSelector,
            string formatString = DebuggerDisplayFormatter.DefaultFormatString,
            int max = DebuggerDisplayFormatter.DefaultEnumerableTake
        )
        {
            return builder.DisplayEnumerable(sourceSelector, x => x, formatString, max);
        }
    }
}