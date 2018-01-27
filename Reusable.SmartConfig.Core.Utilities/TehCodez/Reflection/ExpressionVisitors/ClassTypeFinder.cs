using System;
using System.Linq.Expressions;
using System.Reflection;
using Reusable.Extensions;

namespace Reusable.SmartConfig.Utilities.Reflection
{
    // Finds the type of the class a setting belongs to
    public class ClassTypeFinder : ExpressionVisitor
    {
        private readonly bool _nonPublic;
        private Type _classType;

        private ClassTypeFinder(bool nonPublic) => _nonPublic = nonPublic;

        public static Type FindClassType(Expression expression, bool nonPublic = false)
        {
            var visitor = new ClassTypeFinder(nonPublic);
            visitor.Visit(expression);
            return visitor._classType;
        }

        public override Expression Visit(Expression node)
        {
            // Skip other expressions if we already found the class-type.
            return _classType.IsNull() ? base.Visit(node) : node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // Supports:
            // - static fields
            // - static and instance properties

            switch (node.Member)
            {
                case FieldInfo field when field.IsStatic:
                case PropertyInfo property when property.GetGetMethod(_nonPublic).IsStatic:
                    _classType = node.Member.DeclaringType;
                    break;
                case PropertyInfo property:
                    _classType = node.Member.DeclaringType;
                    break;
            }
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Supports:
            // - closures
            // - this.Members

            var isClosureClass = node.Type.Name.StartsWith("<>c__DisplayClass");
            _classType = isClosureClass ? node.Type.GetFields()[0].FieldType : node.Type;
            return base.VisitConstant(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // Supports:
            // - types passed via generics like .From<T>().Select(x => x.Y);
            _classType = node.Type;
            return base.VisitParameter(node);
        }
    }
}