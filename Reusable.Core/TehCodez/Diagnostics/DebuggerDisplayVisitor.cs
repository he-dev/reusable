using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Reusable.Diagnostics
{
    internal class DebuggerDisplayVisitor : ExpressionVisitor, IEnumerable<MemberExpression>
    {
        private readonly IList<MemberExpression> _members = new List<MemberExpression>();

        public static IEnumerable<MemberExpression> FindMembers(Expression expression)
        {
            var memberFinder = new DebuggerDisplayVisitor();
            memberFinder.Visit(expression);
            return memberFinder;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _members.Add(node);
            return base.VisitMember(node);
        }

        #region IEnumerable<MemberExpression>

        public IEnumerator<MemberExpression> GetEnumerator()
        {
            return _members.Reverse().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}