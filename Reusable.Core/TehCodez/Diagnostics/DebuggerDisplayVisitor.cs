using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Reusable.Diagnostics
{
    internal class DebuggerDisplayVisitor : ExpressionVisitor, IEnumerable<Expression>
    {
        private readonly IList<Expression> _members = new List<Expression>();

        public static IEnumerable<Expression> FindMembers(Expression expression)
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

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _members.Add(node);
            return base.VisitMethodCall(node);
        }

        #region IEnumerable<MemberExpression>

        public IEnumerator<Expression> GetEnumerator()
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