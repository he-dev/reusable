using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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

        public DebuggerDisplayBuilder<T> Property<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var memberExpressions = DebuggerDisplayVisitor.FindMembers(expression);
            var getProperty = expression.Compile();

            return Add(
                memberName: memberExpressions.FormatMemberName(),
                getValue: obj => getProperty(obj).FormatValue()
            );
        }

        public DebuggerDisplayBuilder<T> Collection<TProperty, TValue>(Expression<Func<T, IEnumerable<TProperty>>> expression, Expression<Func<TProperty, TValue>> formatExpression)
        {
            var memberExpressions = DebuggerDisplayVisitor.FindMembers(expression);
            var getProperty = expression.Compile();
            var getValue = formatExpression.Compile();

            return Add(
                memberName: memberExpressions.FormatMemberName(),
                getValue: obj => getProperty(obj).Select(getValue).FormatCollection()
            );
        }

        public DebuggerDisplayBuilder<T> Collection<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> expression)
        {
            return Collection(expression, x => x);
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
}
