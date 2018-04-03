using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Reusable.Diagnostics
{
    internal class DebuggerDisplayVisitor : ExpressionVisitor, IEnumerable<Expression>
    {
        // Member expressions are visited in revers order. 
        // This allows fast inserts at the beginning and thus to avoid reversing it back.
        private readonly LinkedList<Expression> _members = new LinkedList<Expression>();

        public static IEnumerable<Expression> EnumerateMembers(Expression expression)
        {
            var memberFinder = new DebuggerDisplayVisitor();
            memberFinder.Visit(expression);
            return memberFinder;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _members.AddFirst(node);
            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            _members.AddFirst(node);
            return base.VisitMethodCall(node);
        }

        #region IEnumerable<MemberExpression>

        public IEnumerator<Expression> GetEnumerator()
        {
            return _members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}