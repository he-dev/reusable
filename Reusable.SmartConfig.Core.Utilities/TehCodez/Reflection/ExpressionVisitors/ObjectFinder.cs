using System.Linq.Expressions;
using Reusable.Extensions;

namespace Reusable.SmartConfig.Reflection
{
    public class ObjectFinder : ExpressionVisitor
    {
        private object _obj;

        private ObjectFinder() { }

        public static object FindObject(Expression expression)
        {
            var visitor = new ObjectFinder();
            visitor.Visit(expression);
            return visitor._obj;
        }

        public override Expression Visit(Expression node)
        {
            return _obj.IsNull() ? base.Visit(node) : node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var isClosureClass = node.Type.Name.StartsWith("<>c__DisplayClass");
            _obj = isClosureClass ? node.Type.GetFields()[0].GetValue(node.Value) : node.Value;
            return base.VisitConstant(node);
        }
    }
}