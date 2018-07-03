using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Reusable.Extensions;
using Reusable.Reflection;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Utilities.Reflection
{
    // Finds the type of the class a setting belongs to.
    internal class SettingNameFactory : ExpressionVisitor
    {
        private readonly bool _nonPublic;
        private Type _type;
        //private object _instance;
        private MemberInfo _member;
        private string _closureMemberName;

        private SettingNameFactory(bool nonPublic) => _nonPublic = nonPublic;

        public static SettingName CreateSettingName(LambdaExpression expression, bool nonPublic = false)
        {
            //if (!(expression is LambdaExpression)) throw new Exception();

            var visitor = new SettingNameFactory(nonPublic);
            visitor.Visit(expression);

            if (visitor._type is null)
            {
                throw ("UnsupportedSettingExpressionException", "Member's declaring type could not be determined.").ToDynamicException();
            }

            //var member =
            //    lambdaExpression.Body is MemberExpression memberExpression
            //        ? memberExpression.Member
            //        : throw new Exception();

            var settingName = visitor._member.GetCustomAttribute<SmartSettingAttribute>()?.Name ?? visitor._member.Name;

            return new SettingName(settingName)
            {
                Namespace = visitor._type.Namespace,
                Type = visitor._type.ToPrettyString(),
                //Instance = visitor._instance
            };
        }

        //public override Expression Visit(Expression node)
        //{
        //    // Skip other expressions if we already found the class-type.
        //    return
        //        asdf()
        //            ? base.Visit(node)
        //            : node;

        //    bool asdf() => _type is null || _instance is null;
        //}

        protected override Expression VisitMember(MemberExpression node)
        {
            // Supports:
            // - static fields
            // - static and instance properties

            // The first member is the setting.
            _member = _member ?? node.Member;

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
                //_instance = closureType.GetValue(node.Value);
            }
            else
            {
                _type = node.Value.GetType();
                //_instance = node.Value;
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