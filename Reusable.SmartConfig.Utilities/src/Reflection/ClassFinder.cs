using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Reusable.Reflection;

namespace Reusable.SmartConfig.Utilities.Reflection
{
    // Finds the type of the class a setting belongs to.
    internal class ClassFinder : ExpressionVisitor
    {
        private readonly bool _nonPublic;
        private string _memberName;
        private Type _type;
        private object _instance;

        private ClassFinder(bool nonPublic) => _nonPublic = nonPublic;

        public static (Type Type, object Instance) FindClass(Expression expression, bool nonPublic = false)
        {
            var visitor = new ClassFinder(nonPublic);
            visitor.Visit(expression);

            if (visitor._type is null)
            {
                throw ("UnsupportedSettingExpressionException", "Member's declaring type could not be determined.").ToDynamicException();
            }

            return (visitor._type, visitor._instance);
        }

        public override Expression Visit(Expression node)
        {
            // Skip other expressions if we already found the class-type.
            return
                _type is null || _instance is null
                ? base.Visit(node)
                : node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // Supports:
            // - static fields
            // - static and instance properties

            switch (node.Member)
            {
                // (() => Type.Member)
                case FieldInfo field when field.IsStatic:
                case PropertyInfo property when property.GetGetMethod(_nonPublic).IsStatic:
                    _type = node.Member.DeclaringType;
                    break;
                
                // (() => instance.Member) (also this)
                case FieldInfo field:
                case PropertyInfo property:
                    _memberName = node.Member.Name;
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
            _type = isClosureClass ? node.Type.GetField(_memberName).FieldType : node.Value.GetType();
            _instance = isClosureClass ? node.Type.GetField(_memberName).GetValue(node.Value) : node.Value;
            return base.VisitConstant(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // Supports:
            // - types passed via generics like .From<T>().Select(x => x.Y);
            _type = node.Type;
            return base.VisitParameter(node);
        }
    }
}