using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Reusable.Marbles.Diagnostics;

public class DebuggerDisplayBuilder<T>
{
    private readonly IList<(string MemberName, Func<T, object> GetMember)> _members;

    public DebuggerDisplayBuilder()
    {
        _members = new List<(string MemberName, Func<T, object> GetValue)>();
    }

    public DebuggerDisplayBuilder<T> DisplaySingle<TSource>
    (
        Expression<Func<T, TSource>> selector,
        string formatString = DebuggerDisplayFormatter.DefaultFormatString
    )
    {
        var memberExpressions = DebuggerDisplayVisitor.EnumerateMembers(selector);
        var getMember = selector.Compile();

        return Add
        (
            memberName: memberExpressions.FormatMemberName(),
            getMember: obj => obj is {} ? getMember(obj).FormatValue(formatString) : default
        );
    }
        
    public DebuggerDisplayBuilder<T> DisplaySingle<TSource>
    (
        Expression<Func<T, TSource>> selector,
        Func<TSource, string> toString
    )
    {
        var memberExpressions = DebuggerDisplayVisitor.EnumerateMembers(selector);
        var getValue = selector.Compile();

        return Add
        (
            memberName: memberExpressions.FormatMemberName(),
            getMember: obj => obj is {} ? toString(getValue(obj)) : default
        );
    }

    public DebuggerDisplayBuilder<T> DisplayEnumerable<TSource, TItem>
    (
        Expression<Func<T, IEnumerable<TSource>>> sourceSelector,
        Expression<Func<TSource, TItem>> itemSelector,
        string formatString = DebuggerDisplayFormatter.DefaultFormatString,
        int max = DebuggerDisplayFormatter.DefaultEnumerableCount
    )
    {
        var memberExpressions = DebuggerDisplayVisitor.EnumerateMembers(sourceSelector);
        var getProperty = sourceSelector.Compile();
        var getValue = itemSelector.Compile();

        return Add
        (
            memberName: memberExpressions.FormatMemberName(),
            getMember: obj => obj is {} ? getProperty(obj)?.Select(getValue).FormatEnumerable(formatString, max) : default
        );
    }
        
    public DebuggerDisplayBuilder<T> DisplayEnumerable<TSource, TItem>
    (
        Expression<Func<T, IEnumerable<TSource>>> sourceSelector,
        Expression<Func<TSource, TItem>> itemSelector,
        Func<TItem, string> toString,
        int max = DebuggerDisplayFormatter.DefaultEnumerableCount
    )
    {
        var memberExpressions = DebuggerDisplayVisitor.EnumerateMembers(sourceSelector);
        var getProperty = sourceSelector.Compile();
        var getValue = itemSelector.Compile();

        return Add
        (
            memberName: memberExpressions.FormatMemberName(),
            getMember: obj => obj is {} ? getProperty(obj).Select(getValue).Select(toString).FormatEnumerable(DebuggerDisplayFormatter.DefaultFormatString, max) : default
        );
    }

    private DebuggerDisplayBuilder<T> Add(string memberName, Func<T, object> getMember)
    {
        _members.Add((memberName, getMember));
        return this;
    }

    public Func<T, string> Build()
    {
        return obj => string.Join(", ", _members.Select(t => $"{t.MemberName} = {t.GetMember(obj)}"));
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
        int max = DebuggerDisplayFormatter.DefaultEnumerableCount
    )
    {
        return builder.DisplayEnumerable(sourceSelector, x => x, formatString, max);
    }
}