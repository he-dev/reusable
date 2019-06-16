using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Reusable.Exceptionize;

namespace Reusable.Reflection
{
    public class MemberVisitor : ExpressionVisitor
    {
        private readonly bool _nonPublic;
        private Type _type;
        private object _instance;
        private MemberInfo _member;
        private string _closureMemberName;

        private MemberVisitor(bool nonPublic) => _nonPublic = nonPublic;

        public static (Type Type, object Instance, MemberInfo Member) GetMemberInfo(LambdaExpression expression, bool nonPublic = false)
        {
            var visitor = new MemberVisitor(nonPublic);
            visitor.Visit(expression);

            if (visitor._type is null)
            {
                throw ("UnsupportedSettingExpression", "Member's declaring type could not be determined.").ToDynamicException();
            }

            // This fixes the visitor not resolving the overriden member correctly.
            if (visitor._member.ReflectedType != visitor._type)
            {
                visitor._member = 
                    visitor._type.IsInterface 
                        ? visitor._type.FindProperty(visitor._member.Name) 
                        : visitor._type.GetMember(visitor._member.Name).Single();
            }

            return (visitor._type, visitor._instance, visitor._member);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // Supports:
            // - static fields and properties
            // - instance fields and properties

            // The first member is the setting.
            _member = _member ?? node.Member;

            switch (node.Member)
            {
                // (() => Type.Member) - static usage 
                case FieldInfo field when field.IsStatic:
                case PropertyInfo property when property.GetGetMethod(_nonPublic).IsStatic:
                    _type = node.Member.ReflectedType;
                    break;

                // (() => instance.Member) - via instance (also this)
                case FieldInfo field:
                case PropertyInfo property:
                    _closureMemberName = node.Member.Name;
                    break;
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // Supports:
            // - Member (closures)
            // - instance.Member

            if (node.Type.Name.StartsWith("<>c__DisplayClass"))
            {
                var closureType = node.Type.GetField(_closureMemberName);
                _type = closureType.FieldType;
                _instance = closureType.GetValue(node.Value);
            }
            else
            {
                // 'Value' is null when using a 'null' interface selector
                _type = node.Value?.GetType() ?? node.Type;
                _instance = node.Value;
            }

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